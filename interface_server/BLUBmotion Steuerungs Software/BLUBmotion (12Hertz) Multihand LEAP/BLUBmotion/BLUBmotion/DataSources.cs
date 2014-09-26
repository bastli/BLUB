using System;
using System.Timers;
using BLUBmotion;
using System.Drawing;


public class AllOn
{
    string sourcename;
    bool on = false;
    public AllOn()
    {
        sourcename = SourceSwitcher.registerSource("AllOn");



        start_UpdateTimer();
    }

    private void start_UpdateTimer()
    {

        var Timer = new System.Timers.Timer(450);
        Timer.Elapsed += new System.Timers.ElapsedEventHandler(dispatcherTimer_Tick);
        Timer.Enabled = true;
        GC.KeepAlive(Timer);

    }

    private void dispatcherTimer_Tick(object sender, ElapsedEventArgs e)
    {
        BLUBstate telegram = new BLUBstate();

        for (int i = 0; i <= 15; i++)
        {

            if (on)
            {
                telegram.setLED(i, 254);
            }
            else
            {
                telegram.setLED(i, 0);
            }
        }

        for (int i = 0; i <= 61; i++)
        {

            if (on)
            {
                telegram.setValve(i, true);
            }
            else
            {
                telegram.setValve(i, false);
            }
        }



		telegram.isActive = true;


        SourceSwitcher.put(sourcename, telegram);
        on = !on;



    }




}

public class Balken     
{
    string sourcename;
    int location = 0;
    int counter = 0;
    Random rndgen = new Random();

    public Balken()
    {
        sourcename = SourceSwitcher.registerSource("Balken");



        start_UpdateTimer();
    }

    private void start_UpdateTimer()
    {

        var Timer = new System.Timers.Timer(450);
        Timer.Elapsed += new System.Timers.ElapsedEventHandler(dispatcherTimer_Tick);
        Timer.Enabled = true;
        GC.KeepAlive(Timer);

    }

    private void dispatcherTimer_Tick(object sender, ElapsedEventArgs e)
    {
        BLUBstate telegram = new BLUBstate();

        for (int i = 0; i <= 15; i++)
        {

            
                telegram.setLED(i, 254);
            
         
        }

        for (int i = 0; i <= 61; i++)
        {

            if (location <= i && i <= location + 3)
            {
                telegram.setValve(i, true);
            }
            else
            {
                telegram.setValve(i, false);
            }
        }



        telegram.isActive = true;


        SourceSwitcher.put(sourcename, telegram);

        counter++;

        if (counter > 10)
        {
            counter = 0;
            location = rndgen.Next(0, 58);

        }



    }




}

public class Karo
{
    string sourcename;
    bool on = false;
    int counter = 0;
    public Karo()
    {
        sourcename = SourceSwitcher.registerSource("Karo");



        start_UpdateTimer();
    }

    private void start_UpdateTimer()
    {

        var Timer = new System.Timers.Timer(250);
        Timer.Elapsed += new System.Timers.ElapsedEventHandler(dispatcherTimer_Tick);
        Timer.Enabled = true;
        GC.KeepAlive(Timer);

    }

    private void dispatcherTimer_Tick(object sender, ElapsedEventArgs e)
    {
        BLUBstate telegram = new BLUBstate();

        for (int i = 0; i <= 15; i++)
        {

            
                telegram.setLED(i, 254);
      
        }

        for (int i = 0; i <= 61; i++)
        {

            if (on)
            {
                telegram.setValve(i, true);
            }
            else
            {
                telegram.setValve(i, false);
            }
        }
        for (int i = 0; i <= 7; i++)
        {

          
                telegram.setValve(i, on);
                telegram.setValve(i+7, !on);
                telegram.setValve(i+14, on);
                telegram.setValve(i+21, !on);
                telegram.setValve(i + 28, on);
                telegram.setValve(i + 35, !on);
                telegram.setValve(i + 42, on);
                telegram.setValve(i + 49, !on);
                telegram.setValve(Math.Min(i + 56,61), on);
        
        }




		telegram.isActive = true;


        SourceSwitcher.put(sourcename, telegram);

        counter++;

        if (counter > 5)
        {
            counter = 0;
            on = !on;
        }
        



    }




}


public class Fader
{
    string sourcename;
    
    byte counter = 0;

    public Fader()
    {
        sourcename = SourceSwitcher.registerSource("Fader");



        start_UpdateTimer();
    }

    private void start_UpdateTimer()
    {

        var Timer = new System.Timers.Timer(5);
        Timer.Elapsed += new System.Timers.ElapsedEventHandler(dispatcherTimer_Tick);
        Timer.Enabled = true;
        GC.KeepAlive(Timer);

    }

    private void dispatcherTimer_Tick(object sender, ElapsedEventArgs e)
    {
        BLUBstate telegram = new BLUBstate();

        for (int i = 0; i <= 15; i++)
        {

          
                telegram.setLED(i, counter);
            
        }

        for (int i = 0; i <= 61; i++)
        {

            if (counter < 40)
            {
                telegram.setValve(i, true);
            }
            else
            {
                telegram.setValve(i, false);
            }
        }



		telegram.isActive = true;


        SourceSwitcher.put(sourcename, telegram);
        counter++;

      



    }




}

public class Randomshit
{
    string sourcename;

    Random rnd1 = new Random();
    byte[] rndbytes = new byte[1];


    public Randomshit()
    {
        sourcename = SourceSwitcher.registerSource("Randomshit");



        start_UpdateTimer();
    }

    private void start_UpdateTimer()
    {

        var Timer = new System.Timers.Timer(250);
        Timer.Elapsed += new System.Timers.ElapsedEventHandler(dispatcherTimer_Tick);
        Timer.Enabled = true;
        GC.KeepAlive(Timer);

    }

    private void dispatcherTimer_Tick(object sender, ElapsedEventArgs e)
    {
        BLUBstate telegram = new BLUBstate();

        for (int i = 0; i <= 15; i++)
        {

            rnd1.NextBytes(rndbytes);
            telegram.setLED(i, rndbytes[0]);

        }

        for (int i = 0; i <= 61; i++)
        {

            rnd1.NextBytes(rndbytes);
                telegram.setValve(i, GetBit(rndbytes[0],1));
     
        }


		telegram.isActive = true;



        SourceSwitcher.put(sourcename, telegram);
        





    }

    public bool GetBit( byte b, int bitNumber)
    {
        return (b & (1 << bitNumber - 1)) != 0;
    }



}

public class ZigZag
{
    string sourcename;
    int counterValue = 0;

    int[] valveCounters = { 12, 10, 8, 6 };
    bool[] valveCounterDir = { false, false, false, false };

    int[] ledCounters = { 12, 8, 4, 0 };
    bool[] ledCounterDir = { false, false, false, false };

    enum Mode
    {
        Forward,
        Reverse,
        Both
    };

    Mode mode = Mode.Both;

    public ZigZag()
    {
        sourcename = SourceSwitcher.registerSource("ZigZag");

        start_UpdateTimer();
    }

    private void start_UpdateTimer()
    {

        var Timer = new System.Timers.Timer(250);
        Timer.Elapsed += new System.Timers.ElapsedEventHandler(dispatcherTimer_Tick);
        Timer.Enabled = true;
        GC.KeepAlive(Timer);

    }

    private void dispatcherTimer_Tick(object sender, ElapsedEventArgs e)
    {
        BLUBstate telegram = new BLUBstate();

        if (mode == Mode.Forward)
        {
            int ledCounter = (counterValue / 4) % 16;
            int valveCounter = counterValue % 62;

            for (int i = 0; i <= 15; i++)
            {
                if (ledCounter == i)
                    telegram.setLED(i, 254);
                else if ((ledCounter - 1 + 16) % 16 == i)
                    telegram.setLED(i, 180);
                else if ((ledCounter - 2 + 16) % 16 == i)
                    telegram.setLED(i, 120);
                else if ((ledCounter - 3 + 16) % 16 == i)
                    telegram.setLED(i, 60);
                else
                    telegram.setLED(i, 30);
            }

            for (int i = 0; i <= 61; i++)
            {
                if (valveCounter == i)
                    telegram.setValve(i, true);
                else if ((valveCounter - 1 + 62) % 62 == i)
                    telegram.setValve(i, true);
                else if ((valveCounter - 2 + 62) % 62 == i)
                    telegram.setValve(i, true);
                else if ((valveCounter - 3 + 62) % 62 == i)
                    telegram.setValve(i, true);
                else
                    telegram.setValve(i, false);
            }
        }
        else if (mode == Mode.Reverse)
        {
            int ledCounter = ((61 - counterValue) / 4) % 16;
            int valveCounter = (61 - counterValue) % 62;

            for (int i = 0; i <= 15; i++)
            {
                if (ledCounter == i)
                    telegram.setLED(i, 254);
                else if ((ledCounter + 1 + 16) % 16 == i)
                    telegram.setLED(i, 180);
                else if ((ledCounter + 2 + 16) % 16 == i)
                    telegram.setLED(i, 120);
                else if ((ledCounter + 3 + 16) % 16 == i)
                    telegram.setLED(i, 60);
                else
                    telegram.setLED(i, 30);
            }

            for (int i = 0; i <= 61; i++)
            {
                if (valveCounter == i)
                    telegram.setValve(i, true);
                else if ((valveCounter + 1 + 62) % 62 == i)
                    telegram.setValve(i, true);
                else if ((valveCounter + 2 + 62) % 62 == i)
                    telegram.setValve(i, true);
                else if ((valveCounter + 3 + 62) % 62 == i)
                    telegram.setValve(i, true);
                else
                    telegram.setValve(i, false);
            }
        }
        else if (mode == Mode.Both)
        {
            for (int i = 0; i <= 15; i++)
            {
                if (ledCounters[0] / 4 == i)
                    telegram.setLED(i, 254);
                else if (ledCounters[1] / 4 == i)
                    telegram.setLED(i, 180);
                else if (ledCounters[2] / 4 == i)
                    telegram.setLED(i, 120);
                else if (ledCounters[3] / 4 == i)
                    telegram.setLED(i, 60);
                else
                    telegram.setLED(i, 30);
            }

            for (int i = 0; i <= 61; i++)
            {
                if (valveCounters[0] == i)
                    telegram.setValve(i, true);
                else if (valveCounters[1] == i)
                    telegram.setValve(i, true);
                else if (valveCounters[2] == i)
                    telegram.setValve(i, true);
                else if (valveCounters[3] == i)
                    telegram.setValve(i, true);
                else
                    telegram.setValve(i, false);
            }

            for (int i = 0; i < 4; i++)
            {
                if (ledCounterDir[i])
                    ledCounters[i]--;
                else
                    ledCounters[i]++;

                if (valveCounterDir[i])
                    valveCounters[i]--;
                else
                    valveCounters[i]++;

                if (valveCounters[i] == 62)
                {
                    valveCounters[i] = 61;
                    valveCounterDir[i] = !valveCounterDir[i];
                }
                else if (valveCounters[i] == -1)
                {
                    valveCounters[i] = 0;
                    valveCounterDir[i] = !valveCounterDir[i];
                }

                if (ledCounters[i] == 62)
                {
                    ledCounters[i] = 61;
                    ledCounterDir[i] = !ledCounterDir[i];
                }
                else if (ledCounters[i] == -1)
                {
                    ledCounters[i] = 0;
                    ledCounterDir[i] = !ledCounterDir[i];
                }
            }
        }
        /*
        counterValue++;

        if (counterValue == 62 * 1)
        {
            if (mode == Mode.Forward)
                mode = Mode.Reverse;
            else
                mode = Mode.Forward;
            counterValue = 0;
        }
         * */

		telegram.isActive = true;

        SourceSwitcher.put(sourcename, telegram);
    }
}


public class Arrows
{
    string sourcename;
    int counterValue = 0;

    public Arrows()
    {
        sourcename = SourceSwitcher.registerSource("Arrows");

        start_UpdateTimer();
    }

    private void start_UpdateTimer()
    {

        var Timer = new System.Timers.Timer(250);
        Timer.Elapsed += new System.Timers.ElapsedEventHandler(dispatcherTimer_Tick);
        Timer.Enabled = true;
        GC.KeepAlive(Timer);

    }

    private void dispatcherTimer_Tick(object sender, ElapsedEventArgs e)
    {
        BLUBstate telegram = new BLUBstate();

        for (int i = 0; i <= 15; i++)
        {
            telegram.setLED(i, 254);
        }

        const int num_counters = 3;
        const int num_blubs = 2;

        const int modulo = 32;

        for (int i = 0; i < modulo; i++)
        {
            bool open = false;

            for (int j = 0; j < num_counters; j++)
            {
                for (int k = 0; k < num_blubs*2; k++)
                {
                    if ( (i + k) % modulo == (counterValue + j * (modulo / num_counters)) % modulo)
                        open = true;
                }
            }

            telegram.setValve(i, open);
            telegram.setValve(61 - i, open);
        }

        counterValue = (counterValue + num_blubs) % modulo;

		telegram.isActive = true;

        SourceSwitcher.put(sourcename, telegram);
    }
}

public class BlubImage
{
    string sourcename;
    int counterValue = 0;
    Bitmap img;

    public BlubImage()
    {
        sourcename = SourceSwitcher.registerSource("BlubImage");

        
        try 
	    {
            img = new Bitmap("blub.jpeg");
	    }
	    catch (Exception)
	    {
            img = new Bitmap(1, 1);
	    }

        start_UpdateTimer();
    }

    private void start_UpdateTimer()
    {

        var Timer = new System.Timers.Timer(250);
        Timer.Elapsed += new System.Timers.ElapsedEventHandler(dispatcherTimer_Tick);
        Timer.Enabled = true;
        GC.KeepAlive(Timer);

    }

    private void dispatcherTimer_Tick(object sender, ElapsedEventArgs e)
    {
        BLUBstate telegram = new BLUBstate();

        for (int i = 0; i < 16; i++)
        {
            telegram.setLED(i, 254);
        }

        for (int i = 0; i < 62; i++)
        {
            if (img.Width <= i)
                telegram.setValve(i, false);
            else
                telegram.setValve(i, img.GetPixel(i, counterValue).GetBrightness() < 0.5);
        }

        counterValue = (counterValue + 1) % img.Height;

		telegram.isActive = true;

        SourceSwitcher.put(sourcename, telegram);
    }
}