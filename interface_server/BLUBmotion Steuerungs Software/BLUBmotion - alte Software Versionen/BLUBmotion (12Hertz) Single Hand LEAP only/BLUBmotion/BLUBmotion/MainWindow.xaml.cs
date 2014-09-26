using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO.Ports;
using Leap;
using System.Timers;
using TCPServerTutorial;
using System.Diagnostics;

namespace BLUBmotion
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>
	/// 

	// defice LED and valve objects
	public class LED {
		int num;
		public byte val;
		public LED(int number)
		{
			num = number;
			val = 0;
		}
	}

	public class Valve
	{
		int num;
		// value:	0 / false	valve is closed.
		//		 	1 / true	valve is open. 
		public bool val;
		public Valve(int number)
		{
			num = number;
			val = false;
		}
	}


	// a BLUB state
	// contains current values for LEDs and valves. 
	public class BLUBstate
	{
		public int numLEDs = 16;
		public int numValves = 62;

		public bool isActive;		
		public int priority; 

		Valve[] valves;
		LED[] LEDs;

		public BLUBstate()
		{
			valves = new Valve[numValves];
			for (int i = 0; i < numValves; ++i)
			{
				valves[i] = new Valve(i);
			}

			LEDs = new LED[numLEDs];
			for (int i = 0; i < numLEDs; ++i)
			{
				LEDs[i] = new LED(i);
			}
			
		
			// assign default values for backwards compatibility
			isActive = true;		
			priority = 4; 

		}

		// set value of one valve/LED
		public void setLED(int num, byte value)
		{
			LEDs[num].val = value;
		}
		public void setValve(int num, bool value)
		{
			valves[num].val = value;
		}

		// get value of one valve/LED
		public byte getLED(int num)
		{
			return LEDs[num].val;
		}
		public bool getValve(int num)
		{
			
			return valves[num].val;
		}
		public byte getValveBlock(int num)
		{
			byte buffer = 0;
			byte boolbuffer = 0;

			for (int i = 0; i < 8; i++)
			{
				if ((num * 8 + i) > 61)
				{
					boolbuffer = 0;
				}
				else
				{
					boolbuffer = Convert.ToByte(valves[num * 8 + i].val);
				}
				
				byte powbuffer = Convert.ToByte(Math.Pow(2, i));
				buffer += (byte)(boolbuffer * powbuffer);

				
				
			}
			return buffer;
		}


		
	} // BLUBstate


	// data source, needed in SourceSwitcher
	public class Source {
		public string name;
		public BLUBstate state;

		Source(string n, BLUBstate s)
		{
			name = n;
			state = s;
		}
	}


	public partial class MainWindow : Window
	{
		static MotionListener listener;
		//public static SourceSwitcher src_switcher;
		public static Controller controller;
		public LEDGraph[] ledgraphs;
		System.Windows.Shapes.Rectangle[] graphrects;
		System.Windows.Shapes.Rectangle[] valverects;
		public SerialPort rs232LED;
		public SerialPort rs232Valve;
		public TCPServer tcpSrv;
		public ZigZag zigzag;
		public AllOn allon;
		public Fader fadeur;
		public Randomshit randshit;
		public Karo karo;
		public Arrows arrows;
		public BlubImage blubImage;
        public Balken balken;
		//int my_src_num; 

        // leDebug:



		public MainWindow()
		{
			// init everything 
			InitializeComponent();
			initialize_GUI();
			initalize_RS232();
			tcpSrv = new TCPServer(); 
		}

		~MainWindow()
		{
			if (controller != null)
			{
				//controller.RemoveListener(listener);
				controller.Dispose();
			}
		}

		private void initialize_GUI()
		{
			// LED Graphs
			graphrects = new System.Windows.Shapes.Rectangle[16];
			for (int i = 0; i <= 15; i++)
			{
				graphrects[i] = new System.Windows.Shapes.Rectangle();
				graphrects[i].Fill = new SolidColorBrush(Colors.Green);
				graphrects[i].Width = 18;
				graphrects[i].Height = 20;
				graphrects[i].VerticalAlignment = VerticalAlignment.Bottom;
				Canvas.SetLeft(graphrects[i], i * 21);
				Canvas.SetBottom(graphrects[i], 50);
				this.ledCanvas.Children.Add(graphrects[i]);
				this.ledCanvas.VerticalAlignment = VerticalAlignment.Bottom;

				Line ledline = new Line();
				ledline.Stroke = System.Windows.Media.Brushes.Gray;
				ledline.X1 = -3;
				ledline.X2 = 336;

				ledline.Y1 = 59;
				ledline.Y2 = 59;

				ledline.SnapsToDevicePixels = true;
				ledline.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

				this.ledCanvas.Children.Add(ledline);

			}

			// Valve Dots

			valverects = new System.Windows.Shapes.Rectangle[62];
			for (int i = 0; i < 62; i++)
			{
				valverects[i] = new System.Windows.Shapes.Rectangle();
				valverects[i].Fill = new SolidColorBrush(Colors.Gray);
				valverects[i].Width = 5;
				valverects[i].Height = 5;
				valverects[i].VerticalAlignment = VerticalAlignment.Bottom;
				Canvas.SetLeft(valverects[i], i * 8);
				Canvas.SetBottom(valverects[i], 5);
				this.ledCanvas.Children.Add(valverects[i]);
				this.ledCanvas.VerticalAlignment = VerticalAlignment.Bottom;



			}
		}

		private void initalize_RS232()
		{



			foreach (string s in SerialPort.GetPortNames())
			{
				rs232ListBoxLED.Items.Add(s);
				rs232ListBoxValve.Items.Add(s);

			}

			if (rs232ListBoxLED.Items.Count >= 2 && rs232ListBoxValve.Items.Count >= 2)
			{
				rs232ListBoxLED.SelectedIndex = 0;
				rs232ListBoxValve.SelectedIndex = 1;
			}
			else
			{
				MessageBox.Show("Hups....nicht genug Serial Ports gefunden!", "Error!");
				Application.Current.Shutdown();
			}
		}

		private void 
			start_Tick()
		{
			DispatcherTimer guiTimer = new DispatcherTimer();
			guiTimer.Tick += new EventHandler(guiTimer_Tick); 
			guiTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
			guiTimer.Start();

			DispatcherTimer ledTimer = new DispatcherTimer();
			ledTimer.Tick += new EventHandler(ledTimer_Tick);
			ledTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
			ledTimer.Start();

			DispatcherTimer valveTimer = new DispatcherTimer();
			valveTimer.Tick += new EventHandler(valveTimer_Tick);
			valveTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
			valveTimer.Start();

			

		}

		private void ledTimer_Tick(object sender, EventArgs e){


			try
			{
				// get data from switcher
				BLUBstate s = SourceSwitcher.get();
				//	BLUBstate t = new BLUBstate; 

				//if(!s.isActive){
				//	SourceSwitcher.chooseRandomSource();
				//	s = SourceSwitcher.get();
				//}

				byte[] value;
				value = new Byte[1];

				// send start byte to RS232
				value[0] = 255;

				try { rs232LED.Write(value, 0, 1); }
				catch (Exception ex)
				{
					MessageBox.Show("Error writing to LED serial port :: " + ex.Message, "Error!");
					Application.Current.Shutdown();
				}


				// send data bytes and set GUI for each LED
				for (int i = 0; i < 16; i++)
				{

					value[0] = s.getLED(i);

					if (value[0] == 0xFF)
						value[0] = 0xFE;

					try { rs232LED.Write(value, 0, 1); }
					catch (Exception ex)
					{
						rs232LED.Close();
						rs232LED.Dispose();
						MessageBox.Show("Error writing to LED serial port :: " + ex.Message, "Error!");
						
						Application.Current.Shutdown();
					}

					graphrects[i].Height = s.getLED(i) / 5;


				}




			}
			catch (Exception ex)
			{
				MessageBox.Show("Fehler bei der Datenverarbeitung:" + ex.Message, "Error!");
			}

		}

		private void valveTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				// get data from switcher
				BLUBstate s = SourceSwitcher.get();
				//	BLUBstate t = new BLUBstate;

				//Debug.WriteLine(s.isActive);

				// check if source is still active, if not switch to a new one.
				//if (!s.isActive)
				//{
				//	SourceSwitcher.chooseRandomSource();
				//	s = SourceSwitcher.get();
				//}

				byte[] value;
				value = new Byte[1];

				for (int i = 0; i <= 61; i++)
				{
					if (s.getValve(i))
					{
						valverects[i].Fill = new SolidColorBrush(Colors.Red);
					}
					else
					{
						valverects[i].Fill = new SolidColorBrush(Colors.Gray);
					}

				}

				for (int i = 0; i < 8; i++)
				{

					value[0] = s.getValveBlock(i);
					try { rs232Valve.Write(value, 0, 1); }
					catch (Exception ex)
					{
						rs232Valve.Close();
						rs232Valve.Dispose();
						MessageBox.Show("Error writing to Valve serial port :: " + ex.Message, "Error!");
						
						Application.Current.Shutdown();
					}

				}



			}
			catch (Exception ex)
			{
				MessageBox.Show("Fehler bei der Datenverarbeitung:" + ex.Message, "Error!");
			}
		}

		private void guiTimer_Tick(object sender, EventArgs e)
		{


			try
			{



				// write hand position to GUI
				this.xLabel.Content = listener.palmPos.x.ToString();
				this.yLabel.Content = listener.palmPos.y.ToString();
				this.zLabel.Content = listener.palmPos.z.ToString();

				if (listener.palmPos.x >= 0)
				{
					this.rightBar.Width = Math.Ceiling(listener.palmPos.x) / 3;
					this.leftBar.Width = 0;

				}
				else
				{
					this.leftBar.Width = Math.Abs(Math.Ceiling(listener.palmPos.x)) / 3;
					this.rightBar.Width = 0;
				}

				this.handsLabel.Content = listener.hands.ToString();

				this.heightRect.Height = Math.Ceiling(listener.palmPos.y / 4);



			}
			catch (Exception ex)
			{
				MessageBox.Show("Fehler bei der Datenverarbeitung:" + ex.Message, "Error!");
			}

			sourceBox_Update();

		}

		private void open_RS232()
		{
			try
			{
				rs232LED = new SerialPort(this.rs232ListBoxLED.SelectedItem.ToString(), 115200, Parity.None, 8, StopBits.Two);
				rs232LED.Handshake = Handshake.None;

				if (!(rs232LED.IsOpen))
					rs232LED.Open();

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error opening/writing to LED serial port :: " + ex.Message, "Error!");
			}

			try
			{
				rs232Valve = new SerialPort(this.rs232ListBoxValve.SelectedItem.ToString(), 19200, Parity.None, 8, StopBits.One);
				rs232Valve.Handshake = Handshake.None;

				if (!(rs232Valve.IsOpen))
					rs232Valve.Open();

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error opening/writing to Valve serial port :: " + ex.Message, "Error!");
			}



		}


	// LeConnect Button Event
	private void Button_Click(object sender, RoutedEventArgs e)
	{
		// disable the button
		this.connectButton.IsEnabled = false; 

		listener = new MotionListener();
		controller = new Controller();
		controller.AddListener(listener);

		   

		//int startPos = -400;

		//ledgraphs = new LEDGraph[16];
		//for (int i = 0; i <= 15; i++ )
		//{
				
		//	ledgraphs[i] = new LEDGraph(300, startPos);
		//	startPos += 50;

		//}



		//leDebug
		//src_switcher = new SourceSwitcher();
		//my_src_num = src_switcher.registerSource("LeapMouschen");



		open_RS232();

		if (rs232LED.IsOpen && rs232Valve.IsOpen) start_Tick();

		// start some default effects
		zigzag = new ZigZag();
		allon = new AllOn();
		fadeur = new Fader();
		randshit = new Randomshit();
		karo = new Karo();
		arrows = new Arrows();
		blubImage = new BlubImage();
        balken = new Balken();

		//SourceSwitcher.smartChooseSource();

        SourceSwitcher.enableAutoSwitch(); 
			

		   
		}

	
		// LeClose Button
		private void Close_Click(object sender, RoutedEventArgs e)
		{
			tcpSrv.Close();
			if (rs232LED != null && rs232LED.IsOpen)
			{
				rs232LED.Close();
				rs232LED.Dispose();
			}
			if (rs232Valve != null && rs232Valve.IsOpen)
			{
				rs232Valve.Close();
				rs232Valve.Dispose();
			}
			Application.Current.Shutdown();
		}

		// main window closed shall do the same.
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			Close_Click(null, null); 
		}


		private void sourceBox_Update()
		{

			List<string> sourceList = SourceSwitcher.listSources();

			if (sourceList.Contains(SourceSwitcher.currentSource))
			{
				for(int i = 0; i < sourceBox.Items.Count; i++){
					if (sourceBox.Items[i].ToString() == SourceSwitcher.currentSource)
						sourceBox.SelectedItem = i;	
				}
			}

			foreach (string s in sourceList)
			{
				if (!sourceBox.Items.Contains(s))
					sourceBox.Items.Add(s);
			}
			for (int i = sourceList.Count - 1; i >= 0; i--)
			{
				if (!sourceList.Contains(sourceBox.Items[i].ToString()))
				{
					sourceBox.Items.RemoveAt(i);
				}
			}


		}

		private void sourceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 1)
			{
				//Debug.WriteLine(e.AddedItems[0].ToString());
				Debug.WriteLine("[sourceBox_SelectionChanged]");
				SourceSwitcher.switchSource(e.AddedItems[0].ToString());
			}
		}

		private void sourceBox_Initialized(object sender, EventArgs e)
		{
			sourceBox.Items.Add("None");
			sourceBox.SelectedItem = sourceBox.Items.GetItemAt(0);
		}

	


	}

	public class LEDGraph
	{
		public int intensity;
		public int position;
		public bool triggered;
		static int threshold = 100;

		public LEDGraph(int intensity,int position)
		{
			this.intensity = intensity;
			this.position = position;
			this.triggered = false;
		}

		public void Decay()
		{
			if (this.intensity > 5)
			{
				intensity -= 5;

			}
			if (this.intensity <= 5)
			{
				intensity = 5;
			}
			if (this.intensity <= threshold)
			{
				this.triggered = false;
			}
		}

		public void setIntensity(int intensity)
		{
			this.intensity = intensity;

			if (this.intensity > threshold)
			{
				this.triggered = true;
			}
			else
			{
				this.triggered = false;
			}

		}

		public void setThreshold(int threshold)
		{
			LEDGraph.threshold = threshold;
		}

		public bool getTriggered()
		{
			return this.triggered;
		}




	}

	class MotionListener : Listener
	{
		public Leap.Vector palmPos = new Leap.Vector(0, 0, 0);
		public int hands;
		string sourcename;
		LEDGraph[] ledgraphs;

		// detect inacitvity timeout
		int inactiveTicks = 0;
		int maxInactiveTicks = 5; 
		

		public override void OnInit(Controller controller)
		{
			
		}

		public override void OnConnect(Controller controller)
		{
			//sourcenum = MainWindow.src_switcher.registerSource("LeapMotion");
			sourcename = SourceSwitcher.registerSource("LeapMotion");
			

			int startPos = -400;

			ledgraphs = new LEDGraph[16];
			for (int i = 0; i <= 15; i++)
			{

				ledgraphs[i] = new LEDGraph(300, startPos);
				startPos += 50;

			}

			start_UpdateTimer();

		}

		public override void OnDisconnect(Controller controller)
		{

		}

		public override void OnExit(Controller controller)
		{

		}

		private void start_UpdateTimer()
		{
			//DispatcherTimer dispatcherTimer = new DispatcherTimer();
			//dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
			//dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
			//dispatcherTimer.Start();
			var Timer = new System.Timers.Timer(10);
			Timer.Elapsed += new System.Timers.ElapsedEventHandler(dispatcherTimer_Tick);
			Timer.Enabled = true;
			GC.KeepAlive(Timer);

		}

		private void dispatcherTimer_Tick(object sender, ElapsedEventArgs e)
		{
			BLUBstate telegram = new BLUBstate();

			int maxInactiveTicks = 400; 

			for (int i = 0; i <= 15; i++)
			{
				// check if hand is above this LED. If yes, turn on LED. 
				if (Math.Abs(ledgraphs[i].position - palmPos.x) < 25 && palmPos.y > 10)
				{
					// ledgraphs[i].intensity =(int)Math.Ceiling(listener.palmPos.y);
					// je höher die hand, desto heller solls sein.
					ledgraphs[i].setIntensity((int)Math.Ceiling(palmPos.y/3));
				}
				if (Math.Abs(ledgraphs[i].position - palmPos.x) < 100 && palmPos.y > 10)
				{
					// ledgraphs[i].intensity =(int)Math.Ceiling(listener.palmPos.y);
					// je höher die hand, desto heller solls sein.
					ledgraphs[i].setIntensity((int)Math.Ceiling(palmPos.y / (3 + 5*Math.Abs(ledgraphs[i].position - palmPos.x) / 100)));
				}
				else
				{
					ledgraphs[i].Decay();
				}
				//ledgraphs[i].intensity /= 10; // scale
				//
				if (ledgraphs[i].getTriggered())
				{
					if(i*4+0 <= 61)
					telegram.setValve(i * 4 + 0,true);
					if (i * 4 + 1 <= 61)
					telegram.setValve(i * 4 + 1,true);
					if (i * 4 + 2 <= 61)
					telegram.setValve(i * 4 + 2,true);
                    if (i * 4 + 3 <= 61)
                        telegram.setValve(i * 4 + 3, true);
                    if (i * 4 + 4 <= 61)
                        telegram.setValve(i * 4 + 4, true);

				//                  graphrects[i].Fill = new SolidColorBrush(Colors.Aquamarine); 
				//            }
				//          else
				//        {
				//          graphrects[i].Fill = new SolidColorBrush(Colors.Green); ;
				}



				// TODO this should be moved to the paert where the data are sent to rs232?


				if (ledgraphs[i].intensity < 255)
				{
					telegram.setLED(i, (byte)(ledgraphs[i].intensity));
					//value[0] = (byte)(ledgraphs[i].intensity);
				}
				else
				{
					telegram.setLED(i, 254);
					//value[0] = 254;
				}

				// leDebug
				//	t.setLED(i, value[0]); 


				// leDebug
				//	value[0] = s.getLED(i);

				//  rs232LED.Write(value, 0, 1);
			}


			// notify SourceSwitcher if we are active. 
			

			if (hands > 0) {
				inactiveTicks = 0; 
				telegram.isActive = true;
			} else {
				inactiveTicks++;
				if (inactiveTicks > maxInactiveTicks)
				{
					telegram.isActive = false;
                    // Debug.WriteLine("[LEAP DispatcherTimer] Set telegram to inactive. "); 
				}
				
			}

			// leap has highest priority.
			telegram.priority = 1; 


			SourceSwitcher.put(sourcename, telegram);


		}

		public override void OnFrame(Controller controller)
		{
			Leap.Frame frame = controller.Frame();

			hands = frame.Hands.Count();

			if (!frame.Hands.IsEmpty)
			{

				Hand hand = frame.Hands[0];

				palmPos = hand.PalmPosition; // buffer, currently global. 

				
			}
			else
			{
				palmPos = Leap.Vector.Zero;
				// Program.formInstance.setPalmPos(palmPos);
			}

			frame.Dispose();



		}



	}


	public struct priorityItem{
		public int priority;
		public string[] sourceNames;

		public priorityItem(int p, string[] s){
			this.priority = p;
			this.sourceNames = s;
		}
			
	}
	
	public static class SourceSwitcher
	// enables to switch between several inputs.
	{
		// könnte man sicher schöner machen und nich so C style...
		//static int maxInputs = 1024;

		// The Source which is currently sent to the output. 
		public static string currentSource = "NONE";

		// last source identifier
		//static int lastAssignedSource = 0; 

		static Dictionary<string,BLUBstate> sourceList;

		// probably not used anymore
		static List<priorityItem> priorityList;

		// Set to true when auto-switch is enabled
		// Auto Switch means a new source will be selected if the current source is inactive.
		static bool autoSwitch = false;

		// timer interval in ms
		private static double interval = 20000; 

		public static void enableAutoSwitch() {
			autoSwitch = true; 
		}
		public static void disableAutoSwitch() {
			autoSwitch = false; 
		}
		public static bool checkAutoSwitch() {
			return autoSwitch;
		}

	
		// not exactly clear if really needed or the same as currentSource... Well it's public but we can also make the currentSource public... 
		//public static string activeSource;

		// list of connected sources
		//KeyValuePair<int, Source>
		//List<KeyValuePair<int, Source>> sources = new List<KeyValuePair<int, Source>>();

		// data buffers: each source has a name and a state
		//static string[] names;
		//static BLUBstate[] states;

		// Constructor
		static SourceSwitcher()
		{
		//	names = new string[maxInputs];
		//	states = new BLUBstate[maxInputs]; 
			sourceList = new Dictionary<string, BLUBstate>();


			// WARNING Hack Ahead:
			// Priority List is currently hard-coded. 
			priorityList = new List<priorityItem>();
			string[] priority1 = {"BLUBapp"};
			//string[] priority2 = { "QC" };
			//string[] priority3 = { "QC" };
			string[] priority4 = { "ZigZag", "AllOn", "Fader", "Randomshit", "Karo", "Arrows" };
			priorityList.Add(new priorityItem(1, priority1));
			//priorityList.Add(new priorityItem(2, priority2));
			//priorityList.Add(new priorityItem(3, priority3));
			priorityList.Add(new priorityItem(4, priority4));
			//activeSource = null;

			// timer which chooses new input from time to time.
			var sourceTimer = new System.Timers.Timer(interval);
			sourceTimer.Elapsed += new System.Timers.ElapsedEventHandler(sourceTimer_Tick);
			sourceTimer.Enabled = true;
			GC.KeepAlive(sourceTimer);

		}

		static void sourceTimer_Tick(object sender, ElapsedEventArgs e)
		{
			if (checkAutoSwitch()) {
				smartChooseSourceForce(); 
			}
				
		}


		// register a new source
		// returns number of the source
		public static string registerSource(string name) //, int priority)
		{
			//int num = lastAssignedSource++; 
			// allocate space for a BLUBstate
		//	names[num] = name;
		//	states[num] = new BLUBstate();

			if (!sourceList.ContainsKey(name))
			{
				// Add to source and priority lists. 
				sourceList.Add(name, new BLUBstate());
				//string[] tmp = { name }; 
				//priorityList.Add(new priorityItem(1, tmp));
				//sources.Add(new KeyValuePair<int, Source>(k, new Source(name, new BLUBstate)));
				// get source number and return it
				Debug.WriteLine("[SourceSwitcher] registered source: " + name);
				//Debug.WriteLine("[SourceSwitcher] New Priority List is: ");
				//Debug.WriteLine(priorityList);
				return name;
			}
			else {
				return null;	
			}
		}


		public static void unregisterSource(string name)
		{
			// free space 
			// TODO omitted, Garbage Collector can do this.
			
			//names[num] = null;
			//states[num] = null;
			if (sourceList.ContainsKey(name))
			{
				sourceList.Remove(name);
				Debug.WriteLine("[SourceSwitcher] unregistered source: " + name);
			}

		}

		public static List<string> listSources()
		{
			List<string> namelist = new List<string>(sourceList.Keys);
			//Debug.WriteLine("[SourceSwitcher] listSources() called.");
			return namelist;
		}

		// receive data from a source
		public static void put(string name, BLUBstate data) {
			//states[src] = data; 
			if (sourceList.ContainsKey(name))
			{
                bool wasActive = sourceList[name].isActive;

                sourceList[name] = data;

                // detect sources which became active
                if (sourceList[name].isActive != wasActive)
                {
                    Debug.WriteLine("[SourceSwitcher] Source " + name + " changed to active=" + data.isActive);
                    SourceSwitcher.smartChooseSourceForce(); 
                }
			}
		}

		// switch source
		public static void switchSource(string name) {
			if (sourceList.ContainsKey(name) && sourceList[name].isActive)
			{
				currentSource = name;
				Debug.WriteLine("[SourceSwitcher] switchSource called, switched to source: " + name);
			}
		}

		// send data to sink
		public static BLUBstate get() {
			BLUBstate ret; 
	

			if (sourceList.ContainsKey(currentSource) && sourceList[currentSource] != null)
			{
				ret = sourceList[currentSource];
				// TODO if autoswitch is enabled, check if currend state is active. if not choose a new source
			} else { // return an empty inactive state
				ret = new BLUBstate();
                ret.isActive = false;
                ret.priority = 999; 
			}


            if (autoSwitch)
            {
                //if (!ret.isActive) {
                smartChooseSource(ret);
                //}
            }



			//if (states.Count() > 0 && states[currentSource] != null) {
			//	return states[currentSource];
			//} else {
			//	return new BLUBstate(); 
			//}
			// if time, implement some transisions, like mean between two inputs, of the latest input, ... 


			return ret; 
		}

		static int getHighestActivePriority() {

			// hithest priority found.
			int prio = 9999;

			// search all items in priorityList
			//foreach(priorityItem item in priorityList){

            // eebug workaround
            //BLUBstate[] val = (BLUBstate[])sourceList.Values;

            foreach (BLUBstate s in sourceList.Values.ToList())
            //foreach (BLUBstate s in val)
            {
				if (s.isActive && s.priority < prio)
					prio = s.priority;
			}
            //Debug.WriteLine("[SourceSwitcher] getHighestActivePriority prio is = " + prio);

			return prio;
		}

		// chooses a random source from the desired priority. 
		static void chooseRandomSource(int desiredPriority)
		{
			Debug.WriteLine("[SourceSwitcher] chooseRandomSource called with p=" + desiredPriority);
			string nextSource = null; 

			//highest_prio = 
			Random rnd = new Random();
			Dictionary<string, BLUBstate> randomised = new Dictionary<string,BLUBstate>();
			
			// randomise source list
			foreach(var item in sourceList.OrderBy(x => rnd.Next())){
				randomised.Add(item.Key,item.Value);
			}
		

			//Debug.WriteLine("[SourceSwitcher] randomised source list is: ");
			//foreach(Object i in randomised)
			//	Debug.WriteLine("[SourceSwitcher]     " + i);


			// find first object with the correct priority.
			foreach (KeyValuePair<string, BLUBstate> i in randomised)
			{
                //Debug.WriteLine("[SourceSwitcher] looking at " + i.Key + " has priority " + i.Value.priority + " and active " + i.Value.isActive);
				if (i.Value.priority == desiredPriority && i.Value.isActive) {
					nextSource = i.Key;
					break; 
				}
			}

			// set new effect
			if (nextSource != null) {
				switchSource(nextSource); 
			}

			//foreach (BLUBstate s in sourceList.Values)
			//{
			//	if (s.priority < prio)
			//		prio = s.priority;
			//}




			//foreach(priorityItem item in priorityList){
			//	string[] randomNames = item.sourceNames.OrderBy(x => rnd.Next()).ToArray();
			//	foreach(string sourceName in randomNames)
			//		if (sourceList.ContainsKey(sourceName) && sourceList[sourceName].isActive)
			//		{
			//			switchSource(sourceName);
			//		//	activeSource = sourceName;
			//			break; 
			//		}

			//}

		}

		// autonomically chooses a new source, this is probably what you want to call from the outside.
		public static void smartChooseSource()
		{
			//Debug.WriteLine("[SourceSwitcher] smartChooseSource() called.");

			// check for higheest prio
			int h_prio = getHighestActivePriority();
			if (get().priority > h_prio)
			{
				chooseRandomSource(h_prio);
			}
		}

        // workaround because smartChooseSource is also called by get... 
        public static void smartChooseSource(BLUBstate currentState)
        {
            //Debug.WriteLine("[SourceSwitcher] smartChooseSource(currentState) called.");

            // check for higheest prio
            int h_prio = getHighestActivePriority();

            if (currentState.priority > h_prio)
            {
                chooseRandomSource(h_prio);
            }
        }

        // workaround because smartChooseSource is also called by get... 
        public static void smartChooseSourceForce()
        {

            // check for higheest prio
            int h_prio = getHighestActivePriority();
            int c_prio;

            if (get().isActive)
            {
                c_prio = get().priority;
            }
            else
            {
                c_prio = 999;
            }

            if (c_prio >= h_prio)
            {
                chooseRandomSource(h_prio);
            }
            Debug.WriteLine("[SourceSwitcher] smartChooseSourceForce(currentState) called. Current prio is " + c_prio + ", highest is " + h_prio);

        }

    }

}
