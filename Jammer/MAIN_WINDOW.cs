#if WINDOWS && USE_FORMS
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;
namespace jammer;
public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.ShowInTaskbar = false;
        this.Load += new EventHandler(Form1_Load);
        this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
        this.Opacity = 0;
        this.Hide();
        // Hook the keyboard
        Program.hook.HookKeyboard();
    }

    void Form1_Load(object? sender, EventArgs e)
    {
        this.Size = new System.Drawing.Size(0, 0);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Unhook the keyboard when the form is closing
        Program.hook.UnhookKeyboard();

        base.OnFormClosing(e);
    }
}
#endif