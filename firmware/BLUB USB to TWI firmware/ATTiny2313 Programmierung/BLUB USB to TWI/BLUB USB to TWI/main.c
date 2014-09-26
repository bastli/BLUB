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
unsigned char g_twiBuf[18];
unsigned char g_counter;

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
	g_counter = 0;
	
	uart_init();
	
	
	USI_TWI_Master_Initialise();
	
	sei();
	
	unsigned char TWI_targetSlaveAddress   = SLAVE_ADDRESS;
	g_twiBuf[0] = (TWI_targetSlaveAddress<<TWI_ADR_BITS) | (FALSE<<TWI_READ_BIT);
	g_twiBuf[1] = 0x82;
	
	led_setup();
	
	unsigned int j;
	for(j = 0; j < 16; j++)
		g_twiBuf[2 + j] = 0x50;
	
	while(1)
	{
		led_setup();
		
		for(j = 0; j < 1000; j++)
		{
			USI_TWI_Start_Transceiver_With_Data( g_twiBuf, 18);
		}
	}
}

void uart_init(void) {
	UBRRH = 0x00;
	UBRRL = 0x08;	// set baudrate to 115200
	UCSRA = (1 << U2X);
	UCSRB = (1 << RXEN) | (1 << TXEN) | (1 << RXCIE);  //Receiver und Transmitter enabled und Receive Complete Interrupt enabled
	UCSRC = (1 << UCSZ1) | (1 << UCSZ0); // 8 bit character size, no parity Byte, 1 stop Byte; 
}

ISR(USART_RX_vect) {
	unsigned char data = UDR;
	
	// check for control character
	if(data == 0xFF) {
		g_counter = 0;
	}
	else if(g_counter != 16)
	{
		g_twiBuf[2 + g_counter] = data;
		g_counter++;
	}
}