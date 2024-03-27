using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ProjectDMG; 
public class DirectBitmap : IDisposable {

    private Bitmap bitmap;
    private Bitmap bitmapCopy;
    public Bitmap Bitmap {
        get
        {
            return bitmap;
        }
        private set
        {
            bitmap = value;
        }
    }
    public Int32[] Bits { get; private set; }
    public bool Disposed { get; private set; }
    public static int Height = 144;
    public static int Width = 160;

    protected GCHandle BitsHandle { get; private set; }

    public DirectBitmap() {
        Bits = new Int32[Width * Height];
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppRgb, BitsHandle.AddrOfPinnedObject());
        bitmapCopy = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppRgb, BitsHandle.AddrOfPinnedObject());
    }

    public byte[] ToByteArray(ImageFormat format)
    {
        using (var stream = new MemoryStream())
        {
            bitmapCopy.Save(stream, format);
            return stream.ToArray();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(int x, int y, int colour) {
        int index = x + (y * Width);
        Bits[index] = colour;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPixel(int x, int y) {
        int index = x + (y * Width);
        return Bits[index];
    }

    public void Dispose() {
        if (Disposed) return;
        Disposed = true;
        bitmap?.Dispose();
        bitmapCopy?.Dispose();
        BitsHandle.Free();
    }
}