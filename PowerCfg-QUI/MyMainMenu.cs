using Leayal.PowerCfg_QUI.Classes;
using System.Diagnostics;
using System.Reflection;

namespace Leayal.PowerCfg_QUI
{
    public partial class MyMainMenu : Form
    {
        private readonly NotifyIcon TrayIcon;
        private readonly ContextMenuStrip TrayIconMenu;
        private readonly List<ToolStripMenuItem> TrayMenuItems;

        private readonly ToolStripSeparator sep1, sep2;
        private readonly ToolStripMenuItem title, cmdOpenWindowDialog, textPlans, exit;
        private readonly Action? ShowIconContextMenu;

        public MyMainMenu()
        {
            this.TrayMenuItems = new List<ToolStripMenuItem>();
            this.TrayIcon = new NotifyIcon() { Visible = false };

            this.sep1 = new ToolStripSeparator();
            this.sep2 = new ToolStripSeparator();
            this.title = new ToolStripMenuItem("Power Plan Configuration") { Enabled = false };
            this.textPlans = new ToolStripMenuItem("Power Plans:") { Enabled = false };
            this.cmdOpenWindowDialog = new ToolStripMenuItem("Open Setting Panel");
            this.exit = new ToolStripMenuItem("Exit");
            this.exit.Click += this.Exit_Click;
            this.cmdOpenWindowDialog.Click += CmdOpenWindowDialog_Click;

            InitializeComponent();
            this.TrayIconMenu = new ContextMenuStrip() { AutoClose = true };
            this.TrayIcon.Icon = this.Icon;
            this.TrayIcon.Text = this.Text;
            this.TrayIcon.ContextMenuStrip = this.TrayIconMenu;

            this.TrayIconMenu.Opening += this.TrayIconMenu_Opening;
            this.TrayIconMenu.Closed += TrayIconMenu_Closed;
            this.TrayIcon.Click += this.TrayIcon_Click;
            // this.ShowInTaskbar= false;

            if (typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic) is MethodInfo mi)
            {
                this.ShowIconContextMenu = (Action)Delegate.CreateDelegate(typeof(Action), this.TrayIcon, mi);
            }
        }

        private static void CmdOpenWindowDialog_Click(object? sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start(Path.GetFullPath("control.exe", Environment.GetFolderPath(Environment.SpecialFolder.System)), "/name Microsoft.PowerOptions")?.Dispose();
                }
                catch { }
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            this.TrayIcon.ContextMenuStrip = null;
            this.TrayIcon.Visible = false;
            this.TrayIcon.Dispose();
            this.TrayIconMenu.Close(ToolStripDropDownCloseReason.CloseCalled);
            this.TrayIconMenu.Dispose();
            base.OnClosed(e);
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void TrayIconMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is ContextMenuStrip menu)
            {
                menu.Items.Clear();
                menu.Items.AddRange(new ToolStripItem[] { this.title, this.cmdOpenWindowDialog, this.sep1, this.textPlans });

                PowerCfg.GetCurrentPowerScheme(out var activeId, out _);
                
                var list = new List<ToolStripMenuItem>();
                foreach (var planId in PowerCfg.GetAllPowerSchemes())
                {
                    var item = new ToolStripMenuItem(PowerCfg.GetPowerSchemeName(planId))
                    {
                        Tag = planId,
                        Checked = (planId == activeId)
                    };
                    item.Click += ItemPlan_Click;
                    list.Add(item);
                    menu.Items.Add(item);
                }
                menu.Tag = list;

                menu.Items.AddRange(new ToolStripItem[] { this.sep2, this.exit });
            }
        }

        private static void TrayIconMenu_Closed(object? sender, ToolStripDropDownClosedEventArgs e)
        {
            if (sender is ContextMenuStrip menu)
            {
                if (menu.Tag is List<ToolStripMenuItem> list)
                {
                    foreach (var item in list)
                    {
                        menu.Items.Remove(item);
                        item.Click -= ItemPlan_Click;
                        item.Dispose();
                    }
                }
            }
        }

        private static void ItemPlan_Click(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is Guid planId)
            {
                PowerCfg.SetCurrentPowerScheme(planId);
            }
        }

        private void TrayIcon_Click(object? sender, EventArgs e)
        {
            if (sender is NotifyIcon icon && icon.ContextMenuStrip is ContextMenuStrip menu)
            {
                if (this.ShowIconContextMenu != null)
                {
                    // Workaround the issue where manually show the menu will not make the menu visible if the handle isn't created.
                    if (!menu.IsHandleCreated)
                    {
                        // Call it first time when handle is not created to init the menu control object.
                        // Then another call after handle creation to actually show the menu.
                        this.ShowIconContextMenu.Invoke();
                    }
                    this.ShowIconContextMenu.Invoke();
                }
            }
        }

        protected override bool ShowWithoutActivation => true;

        protected override void OnShown(EventArgs e)
        {
            this.Hide();
            this.TrayIcon.Visible = true;
            base.OnShown(e);
        }
    }
}