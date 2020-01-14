using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;


namespace percentage
{
    class TrayIcon
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private int batteryPercentage;
        private NotifyIcon notifyIcon;

        public TrayIcon()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();

            notifyIcon = new NotifyIcon();

            // initialize contextMenu
            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });

            // initialize menuItem
            menuItem.Index = 0;
            menuItem.Text = "E&xit";
            menuItem.Click += new System.EventHandler(MenuItem_Click);

            notifyIcon.ContextMenu = contextMenu;

            batteryPercentage = 0;

            notifyIcon.Visible = true;

			iconUpdate();

            Timer timer = new Timer();
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = 2*60*1000; // in miliseconds // Update every 5 minutes
			// timer.Enabled = true;
			timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
			iconUpdate();
		}

		private void iconUpdate()
		{
			PowerStatus powerStatus = SystemInformation.PowerStatus;
			batteryPercentage = (int)Math.Round((powerStatus.BatteryLifePercent * 100), 0);

			Color foreground = Color.White;
			BatteryChargeStatus batteryChargeStatus = SystemInformation.PowerStatus.BatteryChargeStatus;
			bool charging = batteryChargeStatus.HasFlag(BatteryChargeStatus.Charging);
			if (charging)
				foreground = Color.FromArgb(255, 0, 255, 0);
			else if (batteryPercentage < 30) {
				foreground = Color.FromArgb(255, 255, 0, 0);
			} else {
				foreground = Color.FromArgb(255, 255, 255, 255);
			}

			// Color background = Color.Black;
			Color background = Color.Transparent;

			string drawMe = batteryPercentage.ToString();
			Font fontToUse;
			Brush brushToUse = new SolidBrush(foreground);
			Rectangle rect;
			Bitmap bitmapText;
			Graphics g;

			if (batteryPercentage == 100)
			{
				// reduce size to fit "100"
				fontToUse = new Font("Tahoma", 20, FontStyle.Regular, GraphicsUnit.Pixel);
			}
			else
			{
				fontToUse = new Font("Tahoma", 28, FontStyle.Regular, GraphicsUnit.Pixel);
			}

			rect = new Rectangle(-6, 2, 42, 32);
			bitmapText = new Bitmap(32, 32);

			g = Graphics.FromImage(bitmapText);
			g.Clear(background);

			using (SolidBrush brush = new SolidBrush(background))
			{
				g.FillRectangle(brush, 0, 0, 32, 32);
			}
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
			StringFormat sf = new StringFormat();
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;
			g.DrawString(drawMe, fontToUse, brushToUse, rect, sf);
			g.Save();

			System.IntPtr intPtr = bitmapText.GetHicon();
			try
			{
				using (Icon icon = Icon.FromHandle(intPtr))
				{
					notifyIcon.Icon = icon;

					if (!charging) {
						int seconds = SystemInformation.PowerStatus.BatteryLifeRemaining;
						int mins = seconds / 60;
						int hours = mins / 60;
						mins = mins % 60;
						notifyIcon.Text = hours + ":" + mins + " remaining";
					} else {
						notifyIcon.Text = "Charging";
					}
				}
			}
			finally
			{
				DestroyIcon(intPtr);
			}


			//	hIcon = (bitmapText.GetHicon());
			//	notifyIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
			//	notifyIcon.Text = "Remaining " + batteryPercentage.ToString() + "%";

		}

		private void MenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

    }
}
