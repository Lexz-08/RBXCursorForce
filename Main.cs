using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using NovaUI.Controls;

namespace RBXCursorForce
{
	public partial class Main : NovaWindow
	{
		#region Win32

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int X;
			public int Y;

			public static implicit operator Point(POINT p) => new Point(p.X, p.Y);
		}

		private enum FS_MOD : uint
		{
			NONE = 0x0,
			ALT = 0x1,
			CTRL = 0x2,
			SHIFT = 0x4
		}

		[DllImport("user32.dll")]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, FS_MOD fsModifiers, Keys vk);

		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetCursorPos(int x, int y);

		#endregion

		private IntPtr _rbxptr = IntPtr.Zero;
		private bool _forceCursor = false;

		public Main()
		{
			InitializeComponent();

			Timer t = new Timer { Interval = 1 };
			t.Tick += (_, __) =>
			{
				_rbxptr = FindWindow(null, "Roblox");

				if (_rbxptr != IntPtr.Zero || _rbxptr != (IntPtr)0)
				{
					GetWindowRect(_rbxptr, out RECT rect);

					int x = rect.Left;
					int y = rect.Top;
					int w = (rect.Right - rect.Left) / 2;
					int h = (rect.Bottom - rect.Top) / 2;

					if (_forceCursor) SetCursorPos(x + w, y + h);
				}

				GC.Collect();

				label_RobloxStatus.Text = $"{(_rbxptr != IntPtr.Zero || _rbxptr != (IntPtr)0 ? "" : "NOT ")}DETECTED";
				label_RobloxStatus.ForeColor = _rbxptr != IntPtr.Zero || _rbxptr != (IntPtr)0 ? Color.SeaGreen : Color.Red;

				label_CursorForceStatus.Text = _forceCursor ? "ENABLED" : "DISABLED";
				label_CursorForceStatus.ForeColor = _forceCursor ? Color.SeaGreen : Color.Red;
			};
			t.Start();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			int id_toggle = 0xF0AC;

			bool hk_toggle = RegisterHotKey(Handle, id_toggle, FS_MOD.CTRL, Keys.F);

			if (!hk_toggle)
				MessageBox.Show("Failed to register 'toggle' hotkey: CTRL + SHIFT + F", "HotKey Not Binded",
					MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			UnregisterHotKey(Handle, 0xF0AC);
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			const uint WM_HOTKEY = 0x312;

			if (m.Msg == WM_HOTKEY && (int)m.WParam == 0xF0AC)
				_forceCursor = !_forceCursor;
		}
	}
}
