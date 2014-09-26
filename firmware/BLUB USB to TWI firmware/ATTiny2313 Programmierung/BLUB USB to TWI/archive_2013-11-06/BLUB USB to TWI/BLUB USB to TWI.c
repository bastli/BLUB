/*
 * BLUB_USB_to_TWI.c
 *
 * Created: 25.10.2013 15:03:09
 *  Author: David
 */ 

#include <avr/io.h>
#include <avr/interrupt.h>
#define __ATtiny2313__
#include "USI_TWI_Master.h"

unsigned char g_LEDs[16];
unsigned char g_counter = 0;

void uart_init(void);

#define SLAVE_ADDRESS 0x60

void led_setup()
{
	unsigned char TWI_targetSlaveAddress   = SLAVE_ADDRESS;
	
	unsigned char twi_buf[3];
	twi_buf[0] = (TWI_targetSlaveAddress<<TWI_ADR_BITS) | (FALSE<<TWI_READ_BIT);
	twi_buf[1] = 0x00;
	twi_buf[2] = 0x00;
	USI_TWI_Start_Transceiver_With_Data( twi_buf, 3);
	
	twi_buf[1] = 0x14;
	twi_buf[2] = 0xAA;
	USI_TWI_Start_Transceiver_With_Data( twi_buf, 3);
	
	twi_buf[1] = 0x15;
	twi_buf[2] = 0xAA;
	USI_TWI_Start_Transceiver_With_Data( twi_buf, 3);
	
	twi_buf[1] = 0x16;
	twi_buf[2] = 0xAA;
	USI_TWI_Start_Transceiver_With_Data( twi_buf, 3);
	
	twi_buf[1] = 0x17;
	twi_buf[2] = 0xAA;
	USI_TWI_Start_Transceiver_With_Data( twi_buf, 3);
}

int main(void)
{
	uart_init();
	
	
	USI_TWI_Master_Initialise();
	
	sei();
	
	unsigned char TWI_targetSlaveAddress   = SLAVE_ADDRESS;
	unsigned char twi_buf[3];
	twi_buf[0] = (TWI_targetSlaveAddress<<TWI_ADR_BITS) | (FALSE<<TWI_READ_BIT);
	
	led_setup();
	
	unsigned int j;
	for(j = 0; j < 16; j++)
		g_LEDs[j] = 0xFF;
	
	while(1)
	{
		for(j = 0; j < 100; j++)
		{
			led_setup();
			
			unsigned int i;
			for(i = 0; i < 16; i++)
			{
				twi_buf[1] = 0x02 + i;
				twi_buf[2] = g_LEDs[i];
				USI_TWI_Start_Transceiver_With_Data( twi_buf, 3);
			}
		}
	}
}

void uart_init(void) {
	UBRRH = 0x00;
	UBRRL = 0x00;	// Baudrate auf 0.5Mbps
	UCSRA = (1 << U2X); // Baudrate verdoppeln ->1Mbps!!
	UCSRB = (1 << RXEN) | (1 << TXEN) | (1 << RXCIE);  //Receiver und Transmitter enabled und Receive Complete Interrupt enabled
	UCSRC = (1 << UCSZ1) | (1 << UCSZ0); // 8 bit character size, no parity Byte, 1 stop Byte; 
}

ISR(USART_RX_vect) {
	unsigned char data = UDR;
	
	if(data == 0xFF) {
		g_counter = 0;
	}
	else if(g_counter != 16)
	{
		g_LEDs[g_counter] = data;
		g_counter++;
	}
}