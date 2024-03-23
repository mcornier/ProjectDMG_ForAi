using System;
using System.Windows.Forms;

namespace ProjectDMG;

public partial class Form : System.Windows.Forms.Form {

    ProjectDMG dmg;
    IKeyEventGameboyJoypad keyEventGameboyJoypad
    {
        get => dmg.joypad as IKeyEventGameboyJoypad;
    }

    public Form() {
        InitializeComponent();
    }

    private void Form_Load(object sender, EventArgs e) {
        dmg = new ProjectDMG(this);
    }

    private void Key_Down(object sender, KeyEventArgs e) {
        if (dmg.power_switch) keyEventGameboyJoypad.HandleInputDown(e);
    }

    private void Key_Up(object sender, KeyEventArgs e) {
        if (dmg.power_switch) keyEventGameboyJoypad.HandleInputUp(e);
    }

    private void Drag_Drop(object sender, DragEventArgs e) {
        string[] cartNames = (string[])e.Data.GetData(DataFormats.FileDrop);
        dmg.POWER_ON(cartNames[0]);
    }

    private void Drag_Enter(object sender, DragEventArgs e) {
        if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.All;
        dmg.POWER_OFF();
    }

}
