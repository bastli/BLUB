/**********************************************************************************************************
*
*   B L U B   -   B a s t l i ' s   U l t i m a t i v e r   L e g e n d ä r e r   B L U B
*
* This is the firmware for a mulit relais driver. The number of relais is configurable. The data is stored 
* in an external spi flash which is periodically read and data transferd to the relais. The refresh rate is
* configurable. The flash contant might be updated via the serial port and the xmodem-crc protocol. The 
* number of valid lines (one line means one update of all relais) is stored in the first two bytes of the
* flash in MSB-first (big endian) order.
*
* (c) 2010, 2013 Lukas Schrittwieser, Amiv Bastli at ETH Zurich
*
* This program is free software; you can redistribute it and/or modify it under the terms of the 
* GNU General Public License as published by the Free Software Foundation; either version 3 of the 
* License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without 
* even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
* General Public License for more details.
*
* You should have received a copy of the GNU General Public License along with this program; 
* if not, see <http://www.gnu.org/licenses/>
*
**********************************************************************************************************/

/**********************************************************************************************************
*   I N C L U D E S
*/

#include <avr/io.h>
#include <avr/interrupt.h>


/**********************************************************************************************************
*   C O N F I G U R A T I O N   P A R A M E T E R S
*/

// number of bytes received via UART and sent to relais
#define LINE_LENGTH	8

/* update frequency of the relais in hz */
#define FRAME_RATE	1000

// define the size of a bubble (number of frames for which the valve is opened
#define BUBBLE_SIZE	20

// define number ticks (frames) after which a uart transfer times out
#define RX_TIMEOUT		20


/* size of the flash in bytes */
#define FLASH_SIZE	((uint32_t)128*1024)



/* number of bytes stored in the flash before the actual video data */
#define FLASH_HEADER_LENGTH 2


/**********************************************************************************************************
*   H A R D W A R E   I N T E R F A C E   P A R A M E T E R S
*/

/* note: the LOAD signal is inverted by the gate drive, we compensate that here */
#define SET_LOAD() 			(PORTD &= ~(1<<5))
#define CLR_LOAD() 			(PORTD |= (1<<5))
	
#define LED_ON()			(PORTC |= (1<<1))
#define LED_OFF()			(PORTC &= ~(1<<1))
#define LED_TOGGLE()		(PINC |= (1<<1))

#define GET_PUSH_BUTTON()	(PINC & 1)

#define SET_FLASH_CS()		(PORTB |= (1<<1))
#define CLR_FLASH_CS()		(PORTB &= ~(1<<1))
#define SET_FLASH_HOLD()	(PORTD |= (1<<7))
#define CLR_FLASH_HOLD()	(PORTD &= ~(1<<7))


/**********************************************************************************************************
*   C O N S T A N T S 
*/

/* the spi flash is a 25lc1024 (128kByte) from microchip */
/* commands for the flash, most of them are not needed here */
#define FLASH_READ 	0b00000011 	/* Read data from memory array beginning at selected address */
#define FLASH_WRITE 0b00000010 	/* Write data to memory array beginning at selected address */
#define FLASH_WREN  0b00000110 	/* Set the write enable latch (enable write operations) */
#define FLASH_WRDI  0b00000100 	/* Reset the write enable latch (disable write operations) */
#define FLASH_RDSR  0b00000101 	/* Read STATUS register */
#define FLASH_WRSR  0b00000001 	/* Write STATUS register */
#define FLASH_PE    0b01000010 	/* Page Erase  erase one page in memory array */
#define FLASH_SE    0b11011000 	/* Sector Erase  erase one sector in memory array */
#define FLASH_CE    0b11000111		/* Chip Erase  erase all sectors in memory array */
#define FLASH_RDID  0b10101011 	/* Release from Deep power-down and read electronic signature */
#define FLASH_DPD   0b10111001 	/* Deep Power-Down mode */

/* XMODEM */
#define XMODEM_SOH  0x01
#define XMODEM_EOT  0x04
#define XMODEM_ACK  0x06
#define XMODEM_NACK 0x15
#define XMODEM_C 	0x43

#define XMODEM_ST_IDLE	0x00
#define XMODEM_ST_FN	0x01
#define XMODEM_ST_IFN	0x02
#define XMODEM_ST_RX	0x03
#define XMODEM_ST_CRC1	0x04
#define XMODEM_ST_CRC2	0x05
#define XMODEM_ST_ERR	0x06



/**********************************************************************************************************
*   G L O B A L S 
*/

uint8_t	xModemBuffer[129];

// system tick timer, increments with FRAME_RATE
volatile uint32_t sysTick = 0;


volatile uint8_t rxInd = 0;				// uart reception buffer index
volatile uint8_t rxBuf[LINE_LENGTH];		// uart reception buffer
volatile uint32_t rxStart = 0;			// tick timer at start of reception
	

/**********************************************************************************************************
*   P R O T O T Y P E S
*/

void writeRegs (const unsigned char *d);
void init();
uint8_t spiTransfer (uint8_t d);
uint8_t receiveChar (uint8_t *c, uint8_t t);
void waitForTimer();
uint8_t readLine (unsigned char *d, uint16_t ind);
uint8_t readFlash(uint8_t *d, uint32_t adr, uint8_t len);
uint8_t writeFlash(uint8_t *d, uint32_t adr, uint8_t len);
void writeRegs (const unsigned char *d);
void xModemReceiver();
void putchar (char c);



/**********************************************************************************************************
*   I M P L E M E N T A T I O N
*/

int main (void)
{
	/* one data frame to send to the relais */
	unsigned char data[LINE_LENGTH] = {0x00};

	uint16_t cycle=0, numCycles=0;
	/* initalize the hardware components */
	init();
	sei();

	uint8_t ledCnt=0;
	uint8_t i;
	uint32_t bubbleEndTick = 0;
	
	
	// clear line buffer;
	for (int i=0; i<LINE_LENGTH; i++)
		data[i] = 0;
	
	for(cycle=0;1;cycle++)
	{
		// wait for a timeout (occures every 1/FRAME_RATE seconds) 
		waitForTimer();
		
		// inc system tick (time base)
		cli();
		sysTick++;
		sei();
		
		// update the solenoids
		writeRegs(data);
		
		if (bubbleEndTick <= sysTick)
		{
			// bubbles are finished -> close all valves
			for (int i=0; i<LINE_LENGTH; i++)
				data[i] = 0;
		}
		
		// check for new data
		cli();
		if (rxInd == LINE_LENGTH)
		{
			// we received a new frame -> start creating bubbles
			bubbleEndTick = sysTick + BUBBLE_SIZE;
			// flip data because HW is flipped
			for (i=0; i<LINE_LENGTH; i++)
			{
				data[i] = rxBuf[LINE_LENGTH-1-i];	
			}
		}
		
		// check for uart transmission timeouts
		if ((sysTick-rxStart) > RX_TIMEOUT)
		{
			// reset receive index, ie abort reception
			rxInd = 0;
		}
		sei();
		
		// create a 1hz heart beat on the led 
		if (ledCnt == 1)
			LED_TOGGLE();
		if (ledCnt == FRAME_RATE)
			ledCnt = 0;
		else
			ledCnt++;
		
		
				
		
		
		
		
		
		/* WAFAD code (for reference)
		if (cycle>=numCycles)	// note: we need a >= (not a ==) here as numCycles may change unexpectedly 
		{
			cycle = 0;
			// read the number of cycles from the flash 
			uint8_t buf[2];
			// the first two bytes of the flash hold the number of valid lines (=cycles) in the flash 
			if (readFlash(buf, 0, 2))
				numCycles = (((uint16_t)buf[0])<<8) + buf[1];
			else
				numCycles = 0;
		}
		
		// check if the push button was pressed
		if (!GET_PUSH_BUTTON())
		{
			// the button was pressed 
			// turn off all valves 
			for (i=0; i<LINE_LENGTH; i++)
				data[i] = 0;
			writeRegs(data);
			
			// switch to xmodem receiver 
			xModemReceiver();
			
			// force re-read of numCycles from flash
			numCycles = 0;
			continue;
		}*/
	}
	

}

void waitForTimer()
{
	while(!(TIFR1&(1<<OCF1A)))
		asm("nop");
	/* clear the flag */
	TIFR1 |= (1<<OCF1A);
}


ISR(USART_RX_vect)
{
	uint8_t ch = UDR0;
	
	if (rxInd < LINE_LENGTH)
	{
		if (rxInd == 0)
		{
			// this is the first byte, save the current system tick value to measure the reception time
			rxStart = sysTick;	
		}
		// place data in the receive buffer
		rxBuf[rxInd++] = ch;
	}
	else
	{
		// line or buffer overflow, what should we do?	
	}
}

/* read one line of images information from the spi flash memory */
/* *d:		Array to store the data 
   ind:		Index of the line, within 0..((FLASH_SIZE/LINE_LENGTH)-1)
   return:	0 - ind is out of bounds
   			1 - data successfully copied to d
   */
   /*
uint8_t readLine (unsigned char *d, uint16_t ind)
{
	uint32_t adr;
	// calculate the address, each line is LINE_LEGTH bytes long 
	adr = (LINE_LENGTH * ((uint32_t)ind)) + FLASH_HEADER_LENGTH;
	return readFlash(d, adr, LINE_LENGTH);
}*/




/* read some data bytes from the flash
	d       pointer to store the data to
	adr		address to read the first byte from (adr is auto incrmented)
	len		number of bytes to read
*/
uint8_t readFlash(uint8_t *d, uint32_t adr, uint8_t len)
{
	/* deactivate hold condition */
	SET_FLASH_HOLD();
	/* setup correct spi mode */
	SPCR = (0<<SPIE) | (1<<SPE) | (0<<DORD) | (1<<MSTR) | (0<<CPOL) | (0<<CPHA) | (1<<SPR1) | (0<<SPR0);
	SPSR = 0;
	/* check validity of address */
	if (adr >= FLASH_SIZE)
		return 0; 
	/* select flash */
	CLR_FLASH_CS();
	//asm("nop");
	/* send read command */	
	spiTransfer(FLASH_READ);
	/* send the address */
	spiTransfer((adr>>16)&0xff);
	spiTransfer((adr>>8)&0xff);
	spiTransfer(adr&0xff);
	/* read the data bytes */
	for (uint8_t i=0; i<len; i++)
		d[i] = spiTransfer(0);
	/* deactivate the flash */
	SET_FLASH_CS();
	return 1;
}


/* write some data to the flash 
	d 	 	pointer to data
	adr		address to write data at
	len		number of bytes to write
*/
uint8_t writeFlash(uint8_t *d, uint32_t adr, uint8_t len)
{
	uint8_t i;
	/* setup correct spi mode */
	SPCR = (0<<SPIE) | (1<<SPE) | (0<<DORD) | (1<<MSTR) | (0<<CPOL) | (0<<CPHA) | (1<<SPR1) | (0<<SPR0);
	SPSR = 0;
	/* check validity of address */
	if (adr >= FLASH_SIZE)
		return 0;
	/* select flash */
	CLR_FLASH_CS();
	/* send read command */	
	spiTransfer(FLASH_WREN);
	SET_FLASH_CS();
	for (i=0; i<100; i++)
		asm("nop");
	CLR_FLASH_CS();
	spiTransfer(FLASH_WRITE);
	spiTransfer((adr>>16)&0xff);
	spiTransfer((adr>>8)&0xff);
	spiTransfer(adr&0xff);
	for (i=0; i<len; i++)
		spiTransfer(d[i]);
	/* deactivate the flash */
	SET_FLASH_CS();	

	/* wait until the flash was written */
	for (i=0;i<100;i++)
		asm("nop");
	i=1;
	while(i&0x01)
	{
		CLR_FLASH_CS();
		spiTransfer(FLASH_RDSR);
		i = spiTransfer(0x00);	
		SET_FLASH_CS();
		asm("nop");
		asm("nop");
	}

	return 1;
}

/* CRC algorithm */
uint16_t calcCRC(uint8_t c, uint16_t crc)
{
	uint8_t i;
	for (i=0; i<8; i++)
	{
		if (crc & 0x8000) 
		{
			crc <<= 1;
			crc |= ((c&0x80)?1:0);
			crc ^= 0x1021;
		}
		else
		{
			crc <<= 1;
			crc |= ((c&0x80)?1:0);
		}
		c<<=1;
	}
	return crc;
}

void putchar (char c)
{
	while (!(UCSR0A&(1<<UDRE0)))
		asm("nop");
	UDR0 = c;
}


/* receives data via xModem and stores it to the flash memory */
void xModemReceiver()
{
	uint8_t i,j;
	uint8_t frameNumber=1;			/* xmodem frame number */
	uint8_t index=0;				/* data index within packet */
	uint8_t state=XMODEM_ST_IDLE;	/* receiver state machine */
	uint16_t crc=0;
	uint32_t flashIndex=0;			/* address to store the received data in flash */

	LED_ON();
	/* clear the uart receive buffer */
	i = UDR0;
	j = UDR0;

	/* start transfer by sending an C */
	for (i=0; i<10; i++)
	{
		putchar(XMODEM_C);
		/* wait one sec for an answer from the sender */
		if (receiveChar(&j,1))
			break;	/* we got a char, start fsm */
	}
	/* if we arborted the loop we received data from the host, if the loop finished we abort */
	if (i==10)
	{
		LED_OFF();
		return;
	}
	
	/* frame receiving loop */
	while(1)
	{
		switch (state)
		{
			/* idle: wait for a  start of header */
			case XMODEM_ST_IDLE:
				if (j==XMODEM_SOH)
					state = XMODEM_ST_FN;
				if (j==XMODEM_EOT)
				{
					putchar(XMODEM_ACK);
					putchar(XMODEM_ACK);
					putchar(XMODEM_ACK);
					LED_OFF();
					return;
				}
				break;
			/* check frame number */
			case XMODEM_ST_FN:
				if (j==frameNumber)
					state = XMODEM_ST_IFN;
				else 
					state = XMODEM_ST_ERR;
				break;
			/* check inverted frame number */
			case XMODEM_ST_IFN:
				if ((j^frameNumber)==0xff)
				{
					state = XMODEM_ST_RX;
					index = 0;
					crc = 0;
				}
				else 
					state = XMODEM_ST_ERR;
				break;
			case XMODEM_ST_RX:
				xModemBuffer[index++]=j;
				crc = calcCRC(j,crc);
				if (index==128)
					state = XMODEM_ST_CRC1;
				break;
			case XMODEM_ST_CRC1:
				crc = calcCRC(j,crc);
				state = XMODEM_ST_CRC2;
				break;
			case XMODEM_ST_CRC2:
				crc = calcCRC(j,crc);
				if (crc == 0)
				{
					frameNumber++;
					/* write bytes, do this before the ack to implement a flow control */
					writeFlash(xModemBuffer,flashIndex,128);
					
					/* tell the sender that we are ready to accept the next block */
					putchar(XMODEM_ACK);
					flashIndex+=128;
				}
				else
				{
					putchar(XMODEM_NACK);
				}
				state = XMODEM_ST_IDLE;
				break;
			case XMODEM_ST_ERR:
				/* simply do nothing, we ignore incoming chars, at some point the
				   sender will stop which causes a timeout in receiveChar() we will 
				   then send a nack and go back to idle state */
				break;
			default:
				state = XMODEM_ST_IDLE;
		}
		
		/* wait for a char from the sender with timeout */
		if (!receiveChar(&j, 1))
		{
			/* nothing happend, send nack and go to idle */
			state = XMODEM_ST_IDLE;
			putchar(XMODEM_NACK);
			return;
		}

	}
	
	
}

/* receive a char with timeout.
   c - to store the received char
   t - timeout in seconds
   returns 0 on timeout, 1 on success
*/
uint8_t receiveChar (uint8_t *c, uint8_t t)
{
	uint16_t i;
	for (i=0; i<(FRAME_RATE*((uint16_t)t));)
	{
		/* check the uart */
		if (UCSR0A&(1<<RXC0))
		{
			/* we have a byte */
			(*c) = UDR0;
			return 1;
		}
		//waitForTimer();
		if (TIFR1&(1<<OCF1A))
		{
			i++;
			TIFR1 |= (1<<OCF1A);
		}
	}
	return 0;
}

/* transfer one line to the relais driver shift regs */
void writeRegs (const unsigned char *d)
{
	unsigned char uch;	
	// setup correct spi mode
	CLR_LOAD();
	SPCR = (0<<SPIE) | (1<<SPE) | (0<<DORD) | (1<<MSTR) | (1<<CPOL) | (0<<CPHA) | (1<<SPR1) | (1<<SPR0);
	SPSR = 0;
	for (unsigned char i=0;i<LINE_LENGTH;i++)
	{
		uch = spiTransfer(d[i]);	
	}
	SET_LOAD();
	
}

/* send a byte per spi, the received data is returned */
uint8_t spiTransfer (uint8_t d)
{
	SPDR = d;
		while (!(SPSR&(1<<SPIF)))
			asm("nop");
	return (SPDR);
}


void init()
{
	CLR_LOAD();

	/* setup timer 2: ctc 10ms, div by 64 prescaler */
	OCR1A  = (uint16_t)((((int32_t)F_CPU)/(((uint32_t)64)*FRAME_RATE))-1);
	TIMSK1 = 0;	
	TCCR1A = (0<<WGM11) | (0<<WGM10);
	TCCR1B = (0<<WGM13) | (1<<WGM12) | (0<<CS22) | (1<<CS21) | (1<<CS20);
	
	/* init uart: 19200 baud, 8 data bits, 1 stop bit, no parity */
	UCSR0A = (0<<U2X0);
	UCSR0B = (1<<RXCIE0) | (1<<RXEN0) | (1<<TXEN0);	// enable RX interrupt
	UCSR0C = (1<<UCSZ01) | (1<<UCSZ00);
	UBRR0 = 51;
	
	DDRD = (1<<7) | (1<<5);
	DDRB = (1<<5) | (1<<3) | (1<<1);
	DDRC = (1<<1);	// led output
	
	/* default: all ports with pull ups */
	PORTB = 0xff;
	PORTC = 0xff;
	PORTD = 0xff;
	
	SET_FLASH_CS();
	SET_FLASH_HOLD();
	
	LED_OFF();
}


