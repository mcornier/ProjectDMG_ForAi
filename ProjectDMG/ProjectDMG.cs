using ProjectDMG.OpenAIApi;
using ProjectDMG.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectDMG; 
public class ProjectDMG {

    Form window;

    public ProjectDMG(Form window) {
        this.window = window;
    }

    private CPU cpu;
    private MemoryManagementUnit mmu;
    private PixelProcessingUnit ppu;
    private TIMER timer;
    public IGameboyJoypad joypad;    public InputGameboyJoypad joypadLlm;    public ClientGptPlayer clientGptPlayer;
    public bool power_switch;

    public void POWER_ON(string cartName) {
        mmu = new MemoryManagementUnit();
        cpu = new CPU(mmu);
        ppu = new PixelProcessingUnit(window);
        timer = new TIMER();
        joypad = new QwertyGameboyJoypad();
        joypadLlm = new InputGameboyJoypad();        clientGptPlayer = new ClientGptPlayer("APIKEY");
        mmu.loadGamePak(cartName);

        power_switch = true;

        Task t = Task.Factory.StartNew(EXECUTE, TaskCreationOptions.LongRunning);
    }

    public void POWER_OFF() {
        power_switch = false;
    }

    int fpsCounter;

    public void EXECUTE() {
        // Main Loop Work in progress
        long start = nanoTime();
        long elapsed = 0;
        int cpuCycles = 0;
        int cyclesThisUpdate = 0;

        var timerCounter = new Stopwatch();
        timerCounter.Start();

        while (power_switch) {


            if (timerCounter.ElapsedMilliseconds > 1000)
            {
                window.Invoke((MethodInvoker)(() =>
                {
                    window.Text = "ProjectDMG | FPS: " + fpsCounter;
                }));
                timerCounter.Restart();
                fpsCounter = 0;
            }

            if ((elapsed - start) >= 16740000)
            { //nanoseconds per frame
                start += 16740000;
                while (cyclesThisUpdate < Constants.CYCLES_PER_UPDATE)
                {
                    cpuCycles = cpu.Exe();
                    cyclesThisUpdate += cpuCycles;

                    timer.update(cpuCycles, mmu);
                    ppu.update(cpuCycles, mmu);
                    joypad.Update(mmu);                    handleInterrupts();
                }
                fpsCounter++;
                cyclesThisUpdate -= Constants.CYCLES_PER_UPDATE;

                if (fpsCounter % 100 == 99 && !clientGptPlayer.IsCallingApi)
                {
                    // Conversion de l'image en byte[]
                    byte[] img = ppu.bmp.ToByteArray(ImageFormat.Png);

                    // Chemin où sauvegarder l'image
                    string path = @"C:\Users\micke\Downloads\Pokemon Version Cristal (FR)-1627806688\screen_4_gpt\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";

                    // Sauvegarde de l'image sur le disque
                    File.WriteAllBytes(path, img);

                    window.Invoke((MethodInvoker)(async () => {
                        var r = await clientGptPlayer.CallLlmAsync(img);

                        joypadLlm.HandleInputDown(r);
                        joypadLlm.HandleInputUp(r);
                    }));
                }
            }

            elapsed = nanoTime();
            if ((elapsed - start) < 15000000) {
                Thread.Sleep(1);
            }
        }
    }

    private void handleInterrupts() {
        byte IE = mmu.IE;
        byte IF = mmu.IF;
        for (int i = 0; i < 5; i++) {
            if ((((IE & IF) >> i) & 0x1) == 1) {
                cpu.ExecuteInterrupt(i);
            }
        }

        cpu.UpdateIME();
    }

    private static long nanoTime() {
        long nano = 10000L * Stopwatch.GetTimestamp();
        nano /= TimeSpan.TicksPerMillisecond;
        nano *= 100L;
        return nano;
    }

}