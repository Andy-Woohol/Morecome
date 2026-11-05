// Morecome (몰컴) - 열린 창 하나를 모니터 왼쪽 아래 구석에 작게 띄워 보고, 클릭을 원본으로 전달하는 프로그램
// 빌드: build.cmd (Windows 내장 csc.exe 사용, 별도 설치 불필요)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Morecome
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom;
        public int Width { get { return Right - Left; } }
        public int Height { get { return Bottom - Top; } } }
    [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)]
    public struct DWM_THUMBNAIL_PROPERTIES
    {
        public uint dwFlags; public RECT rcDestination; public RECT rcSource;
        public byte opacity; public int fVisible; public int fSourceClientAreaOnly;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO { public int cbSize; public RECT rcMonitor, rcWork; public uint dwFlags; }
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT { public int dx, dy; public uint mouseData, dwFlags, time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT { public uint type; public MOUSEINPUT mi; }

    static class Native
    {
        public const uint DWM_TNP_RECTDESTINATION = 1, DWM_TNP_RECTSOURCE = 2, DWM_TNP_OPACITY = 4,
                          DWM_TNP_VISIBLE = 8, DWM_TNP_SOURCECLIENTAREAONLY = 16;
        public const int DWMWA_CLOAKED = 14, DWMWA_EXTENDED_FRAME_BOUNDS = 9;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOOLWINDOW = 0x80, WS_EX_APPWINDOW = 0x40000,
                         WS_EX_TRANSPARENT = 0x20, WS_EX_LAYERED = 0x80000;
        public const uint GW_OWNER = 4;
        public const int SW_RESTORE = 9, SW_SHOWNOACTIVATE = 4;
        public const uint SWP_NOZORDER = 4, SWP_NOACTIVATE = 0x10;
        public const uint MONITOR_DEFAULTTONEAREST = 2;
        public const uint INPUT_MOUSE = 0;
        public const uint MOUSEEVENTF_LEFTDOWN = 2, MOUSEEVENTF_LEFTUP = 4, MOUSEEVENTF_WHEEL = 0x800;
        public const uint CWP_SKIPINVISIBLE = 1, CWP_SKIPDISABLED = 2, CWP_SKIPTRANSPARENT = 4;
        public const int WM_KEYDOWN = 0x100, WM_KEYUP = 0x101, WM_CHAR = 0x102, WM_SYSKEYDOWN = 0x104, WM_SYSKEYUP = 0x105, WM_SYSCHAR = 0x106,
                         WM_MOUSEMOVE = 0x200, WM_LBUTTONDOWN = 0x201, WM_LBUTTONUP = 0x202, WM_LBUTTONDBLCLK = 0x203,
                         WM_MOUSEWHEEL = 0x20A, WM_MOUSEHWHEEL = 0x20E;
        public const int MK_LBUTTON = 1;

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("dwmapi.dll")] public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);
        [DllImport("dwmapi.dll")] public static extern int DwmUnregisterThumbnail(IntPtr thumb);
        [DllImport("dwmapi.dll")] public static extern int DwmUpdateThumbnailProperties(IntPtr thumb, ref DWM_THUMBNAIL_PROPERTIES p);
        [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr h, int attr, out int v, int size);
        [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr h, int attr, out RECT r, int size);

        [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc cb, IntPtr lParam);
        [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
        [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr h);
        [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr h);
        [DllImport("user32.dll")] public static extern bool IsZoomed(IntPtr h);
        [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
        [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
        [DllImport("user32.dll")] public static extern IntPtr MonitorFromWindow(IntPtr h, uint flags);
        [DllImport("user32.dll")] public static extern bool GetMonitorInfo(IntPtr mon, ref MONITORINFO mi);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetWindowText(IntPtr h, StringBuilder sb, int max);
        [DllImport("user32.dll")] public static extern int GetWindowTextLength(IntPtr h);
        [DllImport("user32.dll")] public static extern IntPtr GetWindow(IntPtr h, uint cmd);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")] static extern int GetWindowLong32(IntPtr h, int i);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")] static extern IntPtr GetWindowLong64(IntPtr h, int i);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")] static extern int SetWindowLong32(IntPtr h, int i, int v);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")] static extern IntPtr SetWindowLong64(IntPtr h, int i, IntPtr v);
        [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr h, out RECT r);
        [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr h, ref POINT p);
        [DllImport("user32.dll")] public static extern bool ScreenToClient(IntPtr h, ref POINT p);
        [DllImport("user32.dll")] public static extern IntPtr ChildWindowFromPointEx(IntPtr h, POINT p, uint flags);
        [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, int msg, IntPtr w, IntPtr l);
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
        [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
        [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")] public static extern uint SendInput(uint n, INPUT[] inputs, int size);
        [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
        [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
        [DllImport("user32.dll")] public static extern bool SetProcessDpiAwarenessContext(IntPtr ctx);

        public static long GetExStyle(IntPtr h)
        {
            return IntPtr.Size == 8 ? GetWindowLong64(h, GWL_EXSTYLE).ToInt64() : GetWindowLong32(h, GWL_EXSTYLE);
        }
        public static void SetExStyle(IntPtr h, long v)
        {
            if (IntPtr.Size == 8) SetWindowLong64(h, GWL_EXSTYLE, new IntPtr(v)); else SetWindowLong32(h, GWL_EXSTYLE, (int)v);
        }
        public static string WindowTitle(IntPtr h)
        {
            int n = GetWindowTextLength(h); if (n <= 0) return "";
            var sb = new StringBuilder(n + 1); GetWindowText(h, sb, sb.Capacity); return sb.ToString();
        }
        public static bool IsCloaked(IntPtr h)
        {
            int v; return DwmGetWindowAttribute(h, DWMWA_CLOAKED, out v, 4) == 0 && v != 0;
        }
        public static IntPtr MakeLParam(int x, int y) { return new IntPtr((y << 16) | (x & 0xFFFF)); }

        // 최대화된 창을 활성화하지 않고 복원한 뒤, 최대화 때와 똑같은 화면 영역(작업 영역)을 차지하게 맞춘다.
        // 크롬 같은 창은 최대화 상태에서 가려지면 그리기를 멈춰 썸네일이 멎는데, 일반 창이면 계속 그린다.
        public static bool UnmaximizeKeepSize(IntPtr h)
        {
            if (h == IntPtr.Zero || !IsZoomed(h)) return false;
            var mi = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };
            if (!GetMonitorInfo(MonitorFromWindow(h, MONITOR_DEFAULTTONEAREST), ref mi)) return false;
            RECT wa = mi.rcWork;
            ShowWindow(h, SW_SHOWNOACTIVATE);
            // 보이지 않는 테두리(창 사각형과 실제 보이는 프레임의 차이)만큼 바깥으로 키워 눈에 보이는 영역이 작업 영역과 일치하게 한다
            RECT wr, fb; int l = 0, t = 0, r = 0, b = 0;
            if (GetWindowRect(h, out wr) && DwmGetWindowAttribute(h, DWMWA_EXTENDED_FRAME_BOUNDS, out fb, Marshal.SizeOf(typeof(RECT))) == 0)
            { l = fb.Left - wr.Left; t = fb.Top - wr.Top; r = wr.Right - fb.Right; b = wr.Bottom - fb.Bottom; }
            if (l < 0 || l > 20) l = 0; if (t < 0 || t > 20) t = 0; if (r < 0 || r > 20) r = 0; if (b < 0 || b > 20) b = 0;
            return SetWindowPos(h, IntPtr.Zero, wa.Left - l, wa.Top - t, wa.Width + l + r, wa.Height + t + b, SWP_NOZORDER | SWP_NOACTIVATE);
        }
    }

    class WinInfo { public IntPtr Handle; public string Title; }

    class Settings
    {
        public bool TopMost = true; public int SizeIndex = 1; public int Opacity = 100;
        public bool Quiet = true; public bool RestoreCursor = true; public string LastTitle = "";
        public bool Adjust = false; public int CustomW = 0, CustomX = 0, CustomBottom = 0;
        public bool Boss = false; public int BossDelay = 1000, BossIdle = 5, BossHover = 80;   // Boss Nearby: 반응 지연(ms), 평소·올렸을 때 불투명도(%)
        public bool Unmax = true;
        public static string Dir { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Morecome"); } }
        static string PathOf() { return Path.Combine(Dir, "settings.txt"); }
        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                // 예전 이름(MoreCom) 폴더의 설정이 있으면 가져온다
                string old = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MoreCom", "settings.txt");
                if (!File.Exists(PathOf()) && File.Exists(old)) { Directory.CreateDirectory(Dir); File.Copy(old, PathOf()); }
                if (!File.Exists(PathOf())) return s;
                foreach (var line in File.ReadAllLines(PathOf(), Encoding.UTF8))
                {
                    int i = line.IndexOf('='); if (i < 0) continue;
                    string k = line.Substring(0, i), v = line.Substring(i + 1);
                    switch (k)
                    {
                        case "topmost": s.TopMost = v == "1"; break;
                        case "size": int.TryParse(v, out s.SizeIndex); break;
                        case "opacity": int.TryParse(v, out s.Opacity); break;
                        case "quiet": s.Quiet = v == "1"; break;
                        case "restorecursor": s.RestoreCursor = v == "1"; break;
                        case "lasttitle": s.LastTitle = v; break;
                        case "adjust": s.Adjust = v == "1"; break;
                        case "customw": int.TryParse(v, out s.CustomW); break;
                        case "customx": int.TryParse(v, out s.CustomX); break;
                        case "custombottom": int.TryParse(v, out s.CustomBottom); break;
                        case "boss": s.Boss = v == "1"; break;
                        case "bossdelay": int.TryParse(v, out s.BossDelay); break;
                        case "bossidle": int.TryParse(v, out s.BossIdle); break;
                        case "bosshover": int.TryParse(v, out s.BossHover); break;
                        case "unmax": s.Unmax = v == "1"; break;
                    }
                }
            }
            catch { }
            if (s.SizeIndex < 0 || s.SizeIndex > 2) s.SizeIndex = 1;
            if (s.Opacity < 30 || s.Opacity > 100) s.Opacity = 100;
            if (s.BossDelay < 0 || s.BossDelay > 10000) s.BossDelay = 1000;
            if (s.BossIdle < 3 || s.BossIdle > 50) s.BossIdle = 5;          // 0%면 마우스가 창을 통과해 올려도 반응이 없으므로 3% 이상
            if (s.BossHover < 30 || s.BossHover > 100) s.BossHover = 80;
            if (s.BossHover <= s.BossIdle) { s.BossIdle = 5; s.BossHover = 80; }
            return s;
        }
        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(PathOf(),
                    "topmost=" + (TopMost ? "1" : "0") + "\r\nsize=" + SizeIndex + "\r\nopacity=" + Opacity +
                    "\r\nquiet=" + (Quiet ? "1" : "0") + "\r\nrestorecursor=" + (RestoreCursor ? "1" : "0") +
                    "\r\nlasttitle=" + LastTitle + "\r\nadjust=" + (Adjust ? "1" : "0") + "\r\ncustomw=" + CustomW +
                    "\r\ncustomx=" + CustomX + "\r\ncustombottom=" + CustomBottom + "\r\nboss=" + (Boss ? "1" : "0") + "\r\nbossdelay=" + BossDelay + "\r\nbossidle=" + BossIdle + "\r\nbosshover=" + BossHover +
                    "\r\nunmax=" + (Unmax ? "1" : "0") + "\r\n", Encoding.UTF8);
            }
            catch { }
        }
    }

    class MainForm : Form
    {
        const int TitleH = 24, Border = 1, Gap = 8, Grip = 16, MinW = 120;
        static readonly int[] Widths = { 240, 360, 480 };

        // 드래그 조절 상태
        enum DragKind { None, Move, Resize }
        DragKind drag = DragKind.None;
        Point dragStartMouse; Point dragStartLoc; int dragStartW; Size dragStartSize;
        int dragCorner;            // 1=왼쪽 위, 2=오른쪽 위, 3=왼쪽 아래, 4=오른쪽 아래
        bool dragFromButton;       // ▲/✕ 위 모서리에서 끌기 시작했으면 그 클릭은 무시
        ToolStripMenuItem miAdjust;

        Settings cfg;
        IntPtr src = IntPtr.Zero, thumb = IntPtr.Zero;
        int thumbW = 240, thumbH = 150;
        double lastAspect = 0.625;
        RECT lastClient;

        Panel titleBar; Label titleLabel, pinLabel, closeLabel;
        ContextMenuStrip menu; ToolStripMenuItem miWindows, miTop, miSize, miOpacity, miQuiet, miActivate, miRestore, miBoss, miUnmax;
        bool hover; int hoverSince;         // Boss Nearby: 마우스가 창 위에 있는지, 올라온 시각(TickCount)
        int curOpacity = 100;               // 지금 화면에 적용된 불투명도(페이드 중간값 포함)
        int targetOpacity = 100, fadeFrom = 100, fadeStart;   // 페이드 목표값·시작값·시작 시각
        const int FadeMs = 250;
        Timer tick, restoreTimer, fade;
        Point? cursorBackup;
        bool passThrough;
        bool debug;

        // 조용한 모드(메시지 전달) 상태
        IntPtr dragChild = IntPtr.Zero, keyTarget = IntPtr.Zero;

        public MainForm()
        {
            cfg = Settings.Load();
            debug = File.Exists(Path.Combine(Settings.Dir, "debug.on"));
            Text = "제목 없음 - 메모장";   // 작업 표시줄에 메모장처럼 보이게
            FormBorderStyle = FormBorderStyle.None;
            // 작업 표시줄·Alt+Tab 아이콘: exe에 묶어 둔 메모장 아이콘(notepad.ico)
            try
            {
                var rs = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("notepad.ico");
                if (rs != null) Icon = new Icon(rs);
            }
            catch { }
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = true;
            BackColor = Color.FromArgb(32, 32, 32);
            DoubleBuffered = true;
            TopMost = cfg.TopMost;

            titleBar = new Panel { Dock = DockStyle.Top, Height = TitleH, BackColor = Color.FromArgb(45, 45, 48) };
            titleLabel = new Label { Dock = DockStyle.Fill, Text = "우클릭 → 창 선택", ForeColor = Color.Gainsboro,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0), Font = new Font("Segoe UI", 9f), AutoEllipsis = true };
            closeLabel = MakeButton("✕", "닫기"); closeLabel.Click += delegate { if (dragFromButton) { dragFromButton = false; return; } Close(); };
            pinLabel = MakeButton("▲", "항상 위"); pinLabel.Click += delegate { if (dragFromButton) { dragFromButton = false; return; } SetTopMost(!cfg.TopMost); };
            titleBar.Controls.Add(titleLabel); titleBar.Controls.Add(pinLabel); titleBar.Controls.Add(closeLabel);
            Controls.Add(titleBar);

            BuildMenu();
            ContextMenuStrip = menu; titleBar.ContextMenuStrip = menu; titleLabel.ContextMenuStrip = menu;

            MouseDown += OnThumbMouseDown; MouseMove += OnThumbMouseMove; MouseUp += OnThumbMouseUp; MouseWheel += OnThumbWheel;
            Paint += OnPaintThumbArea;
            // 조절 모드: 제목줄을 끌면 이동, 위쪽 두 모서리를 끌면 크기 조절
            foreach (Control c in new Control[] { titleBar, titleLabel, pinLabel, closeLabel })
            {
                bool isButton = c == pinLabel || c == closeLabel;
                Control ctl = c;
                c.MouseDown += delegate(object s, MouseEventArgs e)
                {
                    dragFromButton = false;
                    if (!cfg.Adjust || e.Button != MouseButtons.Left) return;
                    int corner = CornerAt(ToForm(ctl, e.Location));
                    if (corner != 0) { BeginDrag(DragKind.Resize, corner); dragFromButton = isButton; }
                    else if (!isButton) BeginDrag(DragKind.Move, 0);
                };
                c.MouseMove += delegate(object s, MouseEventArgs e)
                {
                    if (cfg.Adjust)
                    {
                        if (drag == DragKind.None)
                        {
                            int corner = CornerAt(ToForm(ctl, e.Location));
                            ctl.Cursor = corner != 0 ? CornerCursor(corner) : isButton ? Cursors.Hand : Cursors.SizeAll;
                        }
                        DoDrag();
                    }
                    else ctl.Cursor = isButton ? Cursors.Hand : Cursors.Default;
                };
                c.MouseUp += delegate(object s, MouseEventArgs e) { EndDrag(); };
            }
            // 위쪽 모서리 손잡이는 제목줄에 가려지므로 제목줄 쪽 컨트롤 위에 다시 그린다
            titleLabel.Paint += delegate(object s, PaintEventArgs e) { PaintGrips(titleLabel, e.Graphics); };
            closeLabel.Paint += delegate(object s, PaintEventArgs e) { PaintGrips(closeLabel, e.Graphics); };

            tick = new Timer { Interval = 150 }; tick.Tick += delegate { Poll(); }; tick.Start();
            restoreTimer = new Timer { Interval = 160 }; restoreTimer.Tick += delegate { EndForward(); };
            fade = new Timer { Interval = 15 }; fade.Tick += delegate { FadeStep(); };

            RefreshPin();
            ApplyOpacity(true);
            Relayout();
        }

        // 목표 불투명도를 정하고 그쪽으로 페이드. Boss Nearby가 켜져 있으면 평소 BossIdle,
        // 마우스가 지연 시간만큼 머물면 BossHover. 아니면 설정한 불투명도.
        void ApplyOpacity() { ApplyOpacity(false); }
        void ApplyOpacity(bool instant)
        {
            int op = cfg.Boss ? (BossLit() ? cfg.BossHover : cfg.BossIdle) : cfg.Opacity;
            if (op == targetOpacity && !instant) return;
            targetOpacity = op;
            if (instant) { fade.Stop(); SetShown(op); return; }
            fadeFrom = curOpacity; fadeStart = Environment.TickCount;
            fade.Start();
        }

        void FadeStep()
        {
            double t = (Environment.TickCount - fadeStart) / (double)FadeMs;
            if (t >= 1) { fade.Stop(); SetShown(targetOpacity); return; }
            SetShown((int)Math.Round(fadeFrom + (targetOpacity - fadeFrom) * t));
        }

        void SetShown(int op)
        {
            if (op == curOpacity) return;
            curOpacity = op;
            Opacity = op / 100.0;
            UpdateThumb();
        }

        // 메뉴가 열려 있거나 드래그 중이면 바로 밝게, 아니면 마우스가 지연 시간만큼 머물렀을 때
        bool BossLit()
        {
            if (!hover) return false;
            if (menu.Visible || drag != DragKind.None) return true;
            return Environment.TickCount - hoverSince >= cfg.BossDelay;
        }

        Label MakeButton(string text, string tip)
        {
            var l = new Label { Dock = DockStyle.Right, Width = 26, Text = text, ForeColor = Color.Silver,
                TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
            new ToolTip().SetToolTip(l, tip);
            l.MouseEnter += delegate { l.BackColor = Color.FromArgb(70, 70, 75); };
            l.MouseLeave += delegate { l.BackColor = Color.Transparent; };
            return l;
        }

        void BuildMenu()
        {
            menu = new ContextMenuStrip();
            miWindows = new ToolStripMenuItem("창 선택");
            miWindows.DropDownOpening += delegate { FillWindowList(); };
            miWindows.DropDownItems.Add("(불러오는 중)");
            miTop = new ToolStripMenuItem("항상 위") { CheckOnClick = true, Checked = cfg.TopMost };
            miTop.CheckedChanged += delegate { SetTopMost(miTop.Checked); };
            miSize = new ToolStripMenuItem("크기");
            string[] names = { "작게 (240px)", "보통 (360px)", "크게 (480px)" };
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                var it = new ToolStripMenuItem(names[i]) { Checked = cfg.SizeIndex == i && cfg.CustomW == 0 };
                it.Click += delegate { cfg.SizeIndex = idx; cfg.CustomW = 0; cfg.Save(); RefreshSizeChecks(); Relayout(); };
                miSize.DropDownItems.Add(it);
            }
            miSize.DropDownItems.Add(new ToolStripSeparator());
            miAdjust = new ToolStripMenuItem("드래그로 크기·위치 조절") { CheckOnClick = true, Checked = cfg.Adjust };
            miAdjust.CheckedChanged += delegate { cfg.Adjust = miAdjust.Checked; cfg.Save(); drag = DragKind.None; Cursor = Cursors.Default; Invalidate(true); };
            miSize.DropDownItems.Add(miAdjust);
            miOpacity = new ToolStripMenuItem("불투명도");
            int[] ops = { 100, 85, 70, 50 };
            foreach (int o in ops)
            {
                int op = o;
                var it = new ToolStripMenuItem(o + "%") { Checked = cfg.Opacity == o };
                it.Click += delegate { cfg.Opacity = op; cfg.Save(); foreach (ToolStripMenuItem m in miOpacity.DropDownItems) m.Checked = false; it.Checked = true; ApplyOpacity(); };
                miOpacity.DropDownItems.Add(it);
            }
            miBoss = new ToolStripMenuItem("Boss Nearby");
            var miBossOn = new ToolStripMenuItem("켜기 (평소 흐리게, 마우스를 올려 두면 밝게)") { CheckOnClick = true, Checked = cfg.Boss };
            miBossOn.CheckedChanged += delegate { cfg.Boss = miBossOn.Checked; cfg.Save(); ApplyOpacity(); };
            var miDelay = Radio("반응 지연", new[] { 0, 500, 1000, 2000, 3000 }, DelayLabel, cfg.BossDelay, delegate(int v) { cfg.BossDelay = v; });
            var miIdle = Radio("평소 불투명도", new[] { 3, 5, 10, 20 }, Pct, cfg.BossIdle, delegate(int v) { cfg.BossIdle = v; });
            var miLit = Radio("마우스 올렸을 때", new[] { 50, 70, 80, 100 }, Pct, cfg.BossHover, delegate(int v) { cfg.BossHover = v; });
            miBoss.DropDownItems.AddRange(new ToolStripItem[] { miBossOn, new ToolStripSeparator(), miDelay, miIdle, miLit });
            miUnmax = new ToolStripMenuItem("선택한 창 최대화 풀기 (크기는 그대로)") { CheckOnClick = true, Checked = cfg.Unmax };
            miUnmax.CheckedChanged += delegate { cfg.Unmax = miUnmax.Checked; cfg.Save(); if (cfg.Unmax) Native.UnmaximizeKeepSize(src); };
            var miMode = new ToolStripMenuItem("클릭 방식");
            miQuiet = new ToolStripMenuItem("원본 창 안 띄우고 전달 (조용히)") { Checked = cfg.Quiet };
            miActivate = new ToolStripMenuItem("원본 창 앞으로 가져와서 클릭") { Checked = !cfg.Quiet };
            miRestore = new ToolStripMenuItem("    클릭 후 마우스 커서 되돌리기") { CheckOnClick = true, Checked = cfg.RestoreCursor, Enabled = !cfg.Quiet };
            miQuiet.Click += delegate { SetQuiet(true); };
            miActivate.Click += delegate { SetQuiet(false); };
            miRestore.CheckedChanged += delegate { cfg.RestoreCursor = miRestore.Checked; cfg.Save(); };
            miMode.DropDownItems.AddRange(new ToolStripItem[] { miQuiet, miActivate, miRestore });
            var miDetach = new ToolStripMenuItem("창 연결 해제"); miDetach.Click += delegate { Attach(IntPtr.Zero); };
            var miExit = new ToolStripMenuItem("종료"); miExit.Click += delegate { Close(); };
            menu.Items.AddRange(new ToolStripItem[] { miWindows, miDetach, new ToolStripSeparator(), miTop, miSize, miOpacity, miBoss, miUnmax, miMode, new ToolStripSeparator(), miExit });
        }

        // 값 하나만 체크되는 하위 메뉴. 고르면 저장하고 불투명도를 다시 적용.
        ToolStripMenuItem Radio(string title, int[] values, Func<int, string> label, int current, Action<int> set)
        {
            var root = new ToolStripMenuItem(title);
            foreach (int v in values)
            {
                int val = v;
                var it = new ToolStripMenuItem(label(v)) { Checked = v == current };
                it.Click += delegate { set(val); cfg.Save(); foreach (ToolStripMenuItem m in root.DropDownItems) m.Checked = false; it.Checked = true; ApplyOpacity(); };
                root.DropDownItems.Add(it);
            }
            return root;
        }
        static string Pct(int v) { return v + "%"; }
        static string DelayLabel(int ms) { return ms == 0 ? "즉시" : (ms / 1000.0).ToString("0.#") + "초"; }

        void RefreshSizeChecks()
        {
            for (int i = 0; i < 3; i++) ((ToolStripMenuItem)miSize.DropDownItems[i]).Checked = cfg.CustomW == 0 && cfg.SizeIndex == i;
        }

        // ---------- 드래그 조절 ----------
        // 폼 좌표 기준으로 어느 모서리 손잡이 위인지 (0 = 모서리 아님)
        int CornerAt(Point p)
        {
            bool l = p.X < Grip, r = p.X >= Width - Grip, t = p.Y < Grip, b = p.Y >= Height - Grip;
            if (t && l) return 1; if (t && r) return 2; if (b && l) return 3; if (b && r) return 4; return 0;
        }
        static Cursor CornerCursor(int c) { return c == 1 || c == 4 ? Cursors.SizeNWSE : Cursors.SizeNESW; }
        Point ToForm(Control c, Point p) { return PointToClient(c.PointToScreen(p)); }

        // 조절 모드일 때 네 모서리의 주황 삼각형 손잡이를 target 컨트롤 위에 그린다
        void PaintGrips(Control target, Graphics g)
        {
            if (!cfg.Adjust) return;
            int W = Width - 1, H = Height - 1;
            var tris = new[] {
                new[] { new Point(0, 0), new Point(Grip, 0), new Point(0, Grip) },
                new[] { new Point(W, 0), new Point(W, Grip), new Point(W - Grip, 0) },
                new[] { new Point(0, H), new Point(0, H - Grip), new Point(Grip, H) },
                new[] { new Point(W, H), new Point(W, H - Grip), new Point(W - Grip, H) } };
            using (var b = new SolidBrush(Color.FromArgb(200, 255, 160, 0)))
                foreach (var tri in tris)
                {
                    var pts = new Point[3];
                    for (int i = 0; i < 3; i++) pts[i] = target == this ? tri[i] : target.PointToClient(PointToScreen(tri[i]));
                    g.FillPolygon(b, pts);
                }
        }

        void BeginDrag(DragKind kind, int corner)
        {
            drag = kind; dragCorner = corner; dragStartMouse = Cursor.Position; dragStartLoc = Location;
            dragStartW = thumbW; dragStartSize = Size;
        }

        void DoDrag()
        {
            if (drag == DragKind.None) return;
            var now = Cursor.Position;
            int dx = now.X - dragStartMouse.X, dy = now.Y - dragStartMouse.Y;
            if (drag == DragKind.Move)
            {
                Location = new Point(dragStartLoc.X + dx, dragStartLoc.Y + dy);
            }
            else
            {
                // 비율 유지: 잡은 모서리의 반대편 모서리를 고정하고, 가로·세로 중 더 많이 움직인 쪽을 따른다
                bool left = dragCorner == 1 || dragCorner == 3, top = dragCorner == 1 || dragCorner == 2;
                int byX = dragStartW + (left ? -dx : dx);
                int byY = dragStartW + (int)Math.Round((top ? -dy : dy) / lastAspect);
                int w = Math.Abs(byX - dragStartW) >= Math.Abs(byY - dragStartW) ? byX : byY;
                var scr = Screen.FromControl(this).WorkingArea;
                int maxW = scr.Width - 2 * Border, maxH = scr.Height - TitleH - Border;
                if (w < MinW) w = MinW; if (w > maxW) w = maxW;
                int h = (int)Math.Round(w * lastAspect);
                if (h > maxH) { h = maxH; w = (int)Math.Round(h / lastAspect); }
                if (h < 40) h = 40;
                thumbW = w; thumbH = h;
                var ns = new Size(thumbW + Border * 2, TitleH + thumbH + Border);
                int x = left ? dragStartLoc.X + dragStartSize.Width - ns.Width : dragStartLoc.X;
                int y = top ? dragStartLoc.Y + dragStartSize.Height - ns.Height : dragStartLoc.Y;
                Bounds = new Rectangle(x, y, ns.Width, ns.Height);
                UpdateThumb(); Invalidate(true);
            }
        }

        void EndDrag()
        {
            if (drag == DragKind.None) return;
            drag = DragKind.None;
            cfg.CustomW = thumbW; cfg.CustomX = Location.X; cfg.CustomBottom = Location.Y + Height; cfg.Save();
            RefreshSizeChecks();
        }

        void SetQuiet(bool q)
        {
            cfg.Quiet = q; cfg.Save();
            miQuiet.Checked = q; miActivate.Checked = !q; miRestore.Enabled = !q;
        }

        // ---------- 창 목록 ----------
        List<WinInfo> ListWindows()
        {
            var list = new List<WinInfo>();
            uint myPid = (uint)Process.GetCurrentProcess().Id;
            Native.EnumWindows(delegate(IntPtr h, IntPtr lp)
            {
                if (h == Handle || !Native.IsWindowVisible(h) || Native.IsCloaked(h)) return true;
                long ex = Native.GetExStyle(h);
                if ((ex & Native.WS_EX_TOOLWINDOW) != 0) return true;
                if (Native.GetWindow(h, Native.GW_OWNER) != IntPtr.Zero && (ex & Native.WS_EX_APPWINDOW) == 0) return true;
                uint pid; Native.GetWindowThreadProcessId(h, out pid); if (pid == myPid) return true;
                string t = Native.WindowTitle(h); if (t.Length == 0) return true;
                list.Add(new WinInfo { Handle = h, Title = t });
                return true;
            }, IntPtr.Zero);
            return list;
        }

        void FillWindowList()
        {
            miWindows.DropDownItems.Clear();
            foreach (var w in ListWindows())
            {
                var wi = w;
                string t = wi.Title.Length > 60 ? wi.Title.Substring(0, 60) + "…" : wi.Title;
                var it = new ToolStripMenuItem(t) { Checked = wi.Handle == src };
                it.Click += delegate { Attach(wi.Handle); };
                miWindows.DropDownItems.Add(it);
            }
            if (miWindows.DropDownItems.Count == 0) miWindows.DropDownItems.Add(new ToolStripMenuItem("(표시할 창 없음)") { Enabled = false });
        }

        // ---------- 썸네일 ----------
        void Attach(IntPtr h)
        {
            if (thumb != IntPtr.Zero) { Native.DwmUnregisterThumbnail(thumb); thumb = IntPtr.Zero; }
            src = h; dragChild = IntPtr.Zero; keyTarget = IntPtr.Zero;
            if (src != IntPtr.Zero)
            {
                if (cfg.Unmax) Native.UnmaximizeKeepSize(src);
                if (Native.DwmRegisterThumbnail(Handle, src, out thumb) != 0) { thumb = IntPtr.Zero; src = IntPtr.Zero; }
                else { cfg.LastTitle = Native.WindowTitle(src); cfg.Save(); }
            }
            Relayout(); Poll(); Invalidate();
        }

        void Relayout()
        {
            bool custom = cfg.CustomW >= MinW;
            int w = custom ? cfg.CustomW : Widths[cfg.SizeIndex];
            RECT cr;
            if (src != IntPtr.Zero && Native.IsWindow(src) && !Native.IsIconic(src) && Native.GetClientRect(src, out cr) && cr.Width > 0 && cr.Height > 0)
            { lastAspect = cr.Height / (double)cr.Width; lastClient = cr; }
            int h = (int)Math.Round(w * lastAspect);
            var wa = Screen.PrimaryScreen.WorkingArea;
            int maxH = custom ? wa.Height - TitleH - Border : (int)(wa.Height * 0.7);
            if (h > maxH) { h = maxH; w = (int)Math.Round(h / lastAspect); }
            if (h < 60) h = 60;
            thumbW = w; thumbH = h;
            Size = new Size(thumbW + Border * 2, TitleH + thumbH + Border);
            if (custom)
            {
                // 저장된 위치(왼쪽·아래 기준)로, 화면 밖이면 안으로 끌어옴
                var vs = SystemInformation.VirtualScreen;
                int x = cfg.CustomX, y = cfg.CustomBottom - Height;
                if (x + Width > vs.Right) x = vs.Right - Width; if (x < vs.Left) x = vs.Left;
                if (y + Height > vs.Bottom) y = vs.Bottom - Height; if (y < vs.Top) y = vs.Top;
                Location = new Point(x, y);
            }
            else Location = new Point(wa.Left + Gap, wa.Bottom - Height - Gap);
            UpdateThumb(); Invalidate();
        }

        void UpdateThumb()
        {
            if (thumb == IntPtr.Zero) return;
            var p = new DWM_THUMBNAIL_PROPERTIES
            {
                dwFlags = Native.DWM_TNP_RECTDESTINATION | Native.DWM_TNP_VISIBLE | Native.DWM_TNP_SOURCECLIENTAREAONLY | Native.DWM_TNP_OPACITY,
                rcDestination = new RECT { Left = Border, Top = TitleH, Right = Border + thumbW, Bottom = TitleH + thumbH },
                opacity = (byte)Math.Round(curOpacity * 2.55), fVisible = 1, fSourceClientAreaOnly = 1
            };
            Native.DwmUpdateThumbnailProperties(thumb, ref p);
        }

        void Poll()
        {
            if (cfg.Boss)
            {
                bool h = Bounds.Contains(Cursor.Position) || menu.Visible || drag != DragKind.None;
                if (h != hover) { hover = h; hoverSince = Environment.TickCount; }
                ApplyOpacity();   // 지연 시간이 지나는 순간을 잡기 위해 매번 호출(목표가 같으면 바로 반환)
            }
            if (src == IntPtr.Zero) { titleLabel.Text = "우클릭 → 창 선택"; return; }
            if (!Native.IsWindow(src)) { Attach(IntPtr.Zero); return; }
            string t = Native.WindowTitle(src); if (titleLabel.Text != t) titleLabel.Text = t;
            RECT cr;
            if (!Native.IsIconic(src) && Native.GetClientRect(src, out cr) && cr.Width > 0 && cr.Height > 0 &&
                (cr.Width != lastClient.Width || cr.Height != lastClient.Height)) Relayout();
        }

        void OnPaintThumbArea(object s, PaintEventArgs e)
        {
            var g = e.Graphics;
            Color border = cfg.Adjust ? Color.FromArgb(255, 160, 0) : cfg.TopMost ? Color.FromArgb(0, 120, 215) : Color.FromArgb(90, 90, 90);
            using (var pen = new Pen(border))
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            PaintGrips(this, g);   // 조절 모드: 네 모서리 손잡이 (위쪽 둘은 제목줄 쪽에서도 다시 그림)
            if (src == IntPtr.Zero)
            {
                var r = new Rectangle(Border, TitleH, thumbW, thumbH);
                TextRenderer.DrawText(g, "표시할 창이 없습니다.\r\n우클릭 → 창 선택", new Font("Segoe UI", 9.5f), r, Color.Gray,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        // ---------- 좌표 변환 ----------
        void Log(string m)
        {
            if (!debug) return;
            try { File.AppendAllText(Path.Combine(Settings.Dir, "debug.log"), DateTime.Now.ToString("HH:mm:ss.fff ") + m + "\r\n"); } catch { }
        }

        bool MapToSource(Point p, out int sx, out int sy)
        {
            sx = sy = 0;
            if (src == IntPtr.Zero || !Native.IsWindow(src)) return false;
            int lx = p.X - Border, ly = p.Y - TitleH;
            if (lx < 0 || ly < 0 || lx >= thumbW || ly >= thumbH) return false;
            RECT cr; if (!Native.GetClientRect(src, out cr) || cr.Width <= 0 || cr.Height <= 0) return false;
            var o = new POINT(); Native.ClientToScreen(src, ref o);
            sx = o.X + (int)Math.Round(lx * (cr.Width / (double)thumbW));
            sy = o.Y + (int)Math.Round(ly * (cr.Height / (double)thumbH));
            return true;
        }

        // 화면 좌표 (sx,sy) 아래의 가장 안쪽 자식 창
        IntPtr ChildAt(int sx, int sy)
        {
            IntPtr cur = src;
            for (int depth = 0; depth < 32; depth++)
            {
                var p = new POINT { X = sx, Y = sy }; Native.ScreenToClient(cur, ref p);
                IntPtr c = Native.ChildWindowFromPointEx(cur, p, Native.CWP_SKIPINVISIBLE | Native.CWP_SKIPDISABLED | Native.CWP_SKIPTRANSPARENT);
                if (c == IntPtr.Zero || c == cur) break;
                cur = c;
            }
            return cur;
        }

        static void PostMouse(IntPtr h, int msg, int keys, int sx, int sy)
        {
            var p = new POINT { X = sx, Y = sy }; Native.ScreenToClient(h, ref p);
            Native.PostMessage(h, msg, new IntPtr(keys), Native.MakeLParam(p.X, p.Y));
        }

        // ---------- 조용한 모드: 메시지 전달 ----------
        void OnThumbMouseDown(object s, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (cfg.Adjust) { int c = CornerAt(e.Location); BeginDrag(c != 0 ? DragKind.Resize : DragKind.Move, c); return; }
            int sx, sy; if (!MapToSource(e.Location, out sx, out sy)) return;
            if (!cfg.Quiet) { ForwardClickActivate(sx, sy); return; }
            if (Native.IsIconic(src)) return;
            dragChild = ChildAt(sx, sy); keyTarget = dragChild;
            Log("down " + sx + "," + sy + " child=" + dragChild.ToString("X") + " clicks=" + e.Clicks);
            PostMouse(dragChild, Native.WM_MOUSEMOVE, 0, sx, sy);
            PostMouse(dragChild, e.Clicks >= 2 ? Native.WM_LBUTTONDBLCLK : Native.WM_LBUTTONDOWN, Native.MK_LBUTTON, sx, sy);
        }

        void OnThumbMouseMove(object s, MouseEventArgs e)
        {
            if (cfg.Adjust)
            {
                if (drag != DragKind.None) DoDrag();
                else { int c = CornerAt(e.Location); Cursor = c != 0 ? CornerCursor(c) : Cursors.SizeAll; }
                return;
            }
            if (Cursor != Cursors.Default) Cursor = Cursors.Default;
            if (!cfg.Quiet || src == IntPtr.Zero) return;
            int sx, sy; if (!MapToSource(e.Location, out sx, out sy)) return;
            if (e.Button == MouseButtons.Left && dragChild != IntPtr.Zero) PostMouse(dragChild, Native.WM_MOUSEMOVE, Native.MK_LBUTTON, sx, sy);
            else if (e.Button == MouseButtons.None && !Native.IsIconic(src)) PostMouse(ChildAt(sx, sy), Native.WM_MOUSEMOVE, 0, sx, sy);
        }

        void OnThumbMouseUp(object s, MouseEventArgs e)
        {
            if (drag != DragKind.None) { EndDrag(); return; }
            if (e.Button != MouseButtons.Left || !cfg.Quiet || dragChild == IntPtr.Zero) return;
            int sx, sy;
            if (!MapToSource(e.Location, out sx, out sy)) { sx = sy = 0; }
            PostMouse(dragChild, Native.WM_LBUTTONUP, 0, sx, sy);
            dragChild = IntPtr.Zero;
        }

        void OnThumbWheel(object s, MouseEventArgs e)
        {
            if (cfg.Adjust) return;
            int sx, sy; if (!MapToSource(e.Location, out sx, out sy) || Native.IsIconic(src)) return;
            if (cfg.Quiet)
            {
                IntPtr h = ChildAt(sx, sy);
                Native.PostMessage(h, Native.WM_MOUSEWHEEL, new IntPtr(e.Delta << 16), Native.MakeLParam(sx, sy));
            }
            else
            {
                BeginForward(sx, sy);
                SendMouse(Native.MOUSEEVENTF_WHEEL, e.Delta);
            }
        }

        // 키보드: 미니 창이 포커스를 가진 동안 누른 키를 마지막 클릭 지점의 창으로 보낸다
        protected override void WndProc(ref Message m)
        {
            if (cfg.Quiet && src != IntPtr.Zero &&
                (m.Msg == Native.WM_KEYDOWN || m.Msg == Native.WM_KEYUP || m.Msg == Native.WM_CHAR ||
                 m.Msg == Native.WM_SYSKEYDOWN || m.Msg == Native.WM_SYSKEYUP || m.Msg == Native.WM_SYSCHAR))
            {
                IntPtr t = keyTarget != IntPtr.Zero && Native.IsWindow(keyTarget) ? keyTarget : src;
                Native.PostMessage(t, m.Msg, m.WParam, m.LParam);
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }

        protected override bool IsInputKey(Keys keyData) { return cfg.Quiet; }

        // ---------- 활성화 모드: 원본을 앞으로 가져와 실제 마우스 입력 ----------
        void ForwardClickActivate(int sx, int sy)
        {
            if (Native.IsIconic(src)) { Native.ShowWindow(src, Native.SW_RESTORE); Native.SetForegroundWindow(src); return; }
            Native.SetForegroundWindow(src);
            BeginForward(sx, sy);
            SendMouse(Native.MOUSEEVENTF_LEFTDOWN, 0); SendMouse(Native.MOUSEEVENTF_LEFTUP, 0);
        }

        void BeginForward(int sx, int sy)
        {
            Log("activate-forward " + sx + "," + sy);
            if (cursorBackup == null) cursorBackup = Cursor.Position;
            var mine = new Rectangle(Location, Size);
            if (mine.Contains(sx, sy) && !passThrough)
            { Native.SetExStyle(Handle, Native.GetExStyle(Handle) | Native.WS_EX_LAYERED | Native.WS_EX_TRANSPARENT); passThrough = true; }
            Native.SetCursorPos(sx, sy);
            restoreTimer.Stop(); restoreTimer.Start();
        }

        void EndForward()
        {
            restoreTimer.Stop();
            if (passThrough) { Native.SetExStyle(Handle, Native.GetExStyle(Handle) & ~(long)Native.WS_EX_TRANSPARENT); passThrough = false; }
            if (cursorBackup != null && cfg.RestoreCursor) Native.SetCursorPos(cursorBackup.Value.X, cursorBackup.Value.Y);
            cursorBackup = null;
        }

        static void SendMouse(uint flags, int data)
        {
            var inp = new INPUT[1];
            inp[0].type = Native.INPUT_MOUSE; inp[0].mi.dwFlags = flags; inp[0].mi.mouseData = (uint)data;
            Native.SendInput(1, inp, Marshal.SizeOf(typeof(INPUT)));
        }

        // ---------- 기타 ----------
        void SetTopMost(bool on)
        {
            cfg.TopMost = on; cfg.Save(); TopMost = on;
            if (miTop.Checked != on) miTop.Checked = on;
            RefreshPin(); Invalidate();
        }
        void RefreshPin()
        {
            pinLabel.Text = cfg.TopMost ? "▲" : "△";
            pinLabel.ForeColor = cfg.TopMost ? Color.FromArgb(80, 160, 255) : Color.Silver;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!string.IsNullOrEmpty(cfg.LastTitle))
                foreach (var w in ListWindows()) if (w.Title == cfg.LastTitle) { Attach(w.Handle); break; }
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (thumb != IntPtr.Zero) Native.DwmUnregisterThumbnail(thumb);
            base.OnFormClosed(e);
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            try { if (!Native.SetProcessDpiAwarenessContext(new IntPtr(-4))) Native.SetProcessDPIAware(); }
            catch { try { Native.SetProcessDPIAware(); } catch { } }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}


