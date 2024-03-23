using ProjectDMG.Utils;
using System.Security.Policy;
using System.Windows.Forms;
using static ProjectDMG.Utils.BitOps;

namespace ProjectDMG; 
//public class JOYPAD {

//    private const int JOYPAD_INTERRUPT = 4;
//    private const byte PAD_MASK = 0x10;
//    private const byte BUTTON_MASK = 0x20;
//    private byte pad = 0xF;
//    private byte buttons = 0xF;

//    internal void handleKeyDown(KeyEventArgs e) {
//        byte b = GetKeyBit(e);
//        if ((b & PAD_MASK) == PAD_MASK) {
//            pad = (byte)(pad & ~(b & 0xF));
//        } else if((b & BUTTON_MASK) == BUTTON_MASK) {
//            buttons = (byte)(buttons & ~(b & 0xF));
//        }
//    }

//    internal void handleKeyUp(KeyEventArgs e) {
//        byte b = GetKeyBit(e);
//        if ((b & PAD_MASK) == PAD_MASK) {
//            pad = (byte)(pad | (b & 0xF));
//        } else if ((b & BUTTON_MASK) == BUTTON_MASK) {
//            buttons = (byte)(buttons | (b & 0xF));
//        }
//    }

//    public void update(MemoryManagementUnit mmu) {
//        byte JOYP = mmu.JOYP;
//        if(!isBit(4, JOYP)) {
//            mmu.JOYP = (byte)((JOYP & 0xF0) | pad);
//            if(pad != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
//        }
//        if (!isBit(5, JOYP)) {
//            mmu.JOYP = (byte)((JOYP & 0xF0) | buttons);
//            if (buttons != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
//        }
//        if ((JOYP & 0b00110000) == 0b00110000) mmu.JOYP = 0xFF;
//    }

//    private byte GetKeyBit(KeyEventArgs e) {
//        switch (e.KeyCode) {
//            case Keys.D:
//            case Keys.Right:
//                return 0x11;

//            case Keys.A:
//            case Keys.Left:
//                return 0x12;

//            case Keys.W:
//            case Keys.Up:
//                return 0x14;

//            case Keys.S:
//            case Keys.Down:
//                return 0x18;

//            case Keys.J:
//            case Keys.Z:
//                return 0x21;

//            case Keys.K:
//            case Keys.X:
//                return 0x22;

//            case Keys.Space:
//            case Keys.C:
//                return 0x24;

//            case Keys.Enter:
//            case Keys.V:
//                return 0x28;
//        }
//        return 0;
//    }
//}

public enum GameboyInputs
{
    None,
    Right,
    Left,
    Up,
    Down,
    A,
    B,
    Select,
    Start
}

public interface IGameboyJoypad
{
    void Update(MemoryManagementUnit mmu);
}

public interface IInputsGameboyJoypad : IGameboyJoypad
{
    void HandleInputDown(params GameboyInputs[] input);
    void HandleInputUp(params GameboyInputs[] input);
}

public interface IKeyEventGameboyJoypad : IGameboyJoypad
{
    void HandleInputDown(KeyEventArgs e);
    void HandleInputUp(KeyEventArgs e);
}

public abstract class GameboyJoypadBase : IGameboyJoypad
{
    private const byte PAD_MASK = 0x10;
    private const byte BUTTON_MASK = 0x20;
    protected byte pad = 0xF; // 4 bits pour les directions
    protected byte buttons = 0xF; // 4 bits pour les boutons A, B, Select, Start
    private const int JOYPAD_INTERRUPT = 4;

    protected void handleKeyDown(GameboyInputs input)
    {
        byte b = ConvertInputToBit(input);
        if ((b & PAD_MASK) == PAD_MASK)
        {
            pad = (byte)(pad & ~(b & 0xF));
        }
        else if ((b & BUTTON_MASK) == BUTTON_MASK)
        {
            buttons = (byte)(buttons & ~(b & 0xF));
        }
    }

    protected void handleKeyUp(GameboyInputs input)
    {
        byte b = ConvertInputToBit(input);
        if ((b & PAD_MASK) == PAD_MASK)
        {
            pad = (byte)(pad | (b & 0xF));
        }
        else if ((b & BUTTON_MASK) == BUTTON_MASK)
        {
            buttons = (byte)(buttons | (b & 0xF));
        }
    }

    // Méthode pour mettre à jour l'état du joypad dans la MMU
    public void Update(MemoryManagementUnit mmu)
    {
        byte JOYP = mmu.JOYP;
        // Vérifie si les directions sont activées
        if (!isBit(4, JOYP))
        {
            mmu.JOYP = (byte)((JOYP & 0xF0) | pad);
            if (pad != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
        }
        // Vérifie si les boutons sont activés
        if (!isBit(5, JOYP))
        {
            mmu.JOYP = (byte)((JOYP & 0xF0) | buttons);
            if (buttons != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
        }
        // Si les deux sont désactivés, mettre à 0xFF
        if ((JOYP & 0b00110000) == 0b00110000) mmu.JOYP = 0xFF;
    }

    // Méthodes utilitaires protégées ou privées pour être utilisées par les sous-classes
    protected void UpdateButtonState(GameboyInputs input, bool isKeyUp)
    {
        byte mask = (byte)((input < GameboyInputs.Select) ? PAD_MASK : BUTTON_MASK);
        byte bit = (byte)(1 << ((byte)input % 4));

        if (isKeyUp)
        {
            if ((mask & PAD_MASK) == PAD_MASK) pad |= bit;
            else if ((mask & BUTTON_MASK) == BUTTON_MASK) buttons |= bit;
        }
        else
        {
            if ((mask & PAD_MASK) == PAD_MASK) pad &= (byte)~bit;
            else if ((mask & BUTTON_MASK) == BUTTON_MASK) buttons &= (byte)~bit;
        }
    }

    protected byte ConvertInputToBit(GameboyInputs input)
    {
        switch (input)
        {
            case GameboyInputs.Right: return 0x11;
            case GameboyInputs.Left: return 0x12;
            case GameboyInputs.Up: return 0x14;
            case GameboyInputs.Down: return 0x18;
            case GameboyInputs.A: return 0x21;
            case GameboyInputs.B: return 0x22;
            case GameboyInputs.Select: return 0x24;
            case GameboyInputs.Start: return 0x28;
            default: return 0;
        }
    }
}

public class InputGameboyJoypad : GameboyJoypadBase, IInputsGameboyJoypad
{
    // Méthodes pour gérer les événements de pression et relâchement des touches
    public void HandleInputDown(params GameboyInputs[] inputs)
    {
        foreach (var input in inputs)
        {
            handleKeyDown(input);
        }        
    }
    public void HandleInputUp(params GameboyInputs[] inputs)
    {
        foreach (var input in inputs)
        {
            handleKeyUp(input);
        }
    }
}

public class QwertyGameboyJoypad : GameboyJoypadBase, IKeyEventGameboyJoypad
{
    public void HandleInputDown(KeyEventArgs e)
    {
        handleKeyDown(ConvertKeyCodeToGameboyInput(e));
    }

    public void HandleInputUp(KeyEventArgs e)
    {
        handleKeyUp(ConvertKeyCodeToGameboyInput(e));
    }

    private GameboyInputs ConvertKeyCodeToGameboyInput(KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            // Directions
            case Keys.Right:
            case Keys.D:
                return GameboyInputs.Right;
            case Keys.Left:
            case Keys.A:
                return GameboyInputs.Left;
            case Keys.Up:
            case Keys.W:
                return GameboyInputs.Up;
            case Keys.Down:
            case Keys.S:
                return GameboyInputs.Down;

            // Boutons
            case Keys.Z:
            case Keys.J: // Bouton A alternatif
                return GameboyInputs.A;
            case Keys.X:
            case Keys.K: // Bouton B alternatif
                return GameboyInputs.B;
            case Keys.ShiftKey: // Sélect
                return GameboyInputs.Select;
            case Keys.Enter: // Start
            case Keys.V: // Start alternatif
                return GameboyInputs.Start;

            // Ajouts basés sur la deuxième liste
            case Keys.Space: // Bouton A alternatif
                return GameboyInputs.A;
            case Keys.C: // Bouton B alternatif
                return GameboyInputs.B;

            default:
                return GameboyInputs.None;
        }
    }
}
