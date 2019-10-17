using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapturingContext
{
    class Program
    {
        const int ITERS = 50000;

        static void Main(string[] args)
        {
            Form f = new Form() { Width = 400, Height = 300 };
            Button b = new Button() { Text = "Run", Dock = DockStyle.Fill, Font = new Font("Consolas", 18) };
            f.Controls.Add(b);

            b.Click += async delegate
            {
                b.Text = "... Running ... ";
                await Task.WhenAll(WithSyncCtx(), WithoutSyncCtx()); // warm-up

                Stopwatch sw = new Stopwatch();

                sw.Restart();
                await WithSyncCtx();
                System.TimeSpan withTime = sw.Elapsed;

                sw.Restart();
                await WithoutSyncCtx();
                System.TimeSpan withoutTime = sw.Elapsed;

                b.Text = string.Format("With    : {0}\nWithout : {1}\n\nDiff    : {2:F2}x",
                    withTime, withoutTime, withTime.TotalSeconds / withoutTime.TotalSeconds);
            };

            f.ShowDialog();
        }
        

        private static async Task WithSyncCtx()
        {
            for (int i = 0; i < ITERS; i++)
            {
                Task t = Task.Run(() => { });
                await t;
            }
        }

        private static async Task WithoutSyncCtx()
        {
            for (int i = 0; i < ITERS; i++)
            {
                Task t = Task.Run(() => { });
                await t.ConfigureAwait(continueOnCapturedContext: false);
            }
        }

    }
}
