using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using epex.mordhau.recode;
using Memory;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;

namespace MordhauCheat_2._0
{
    public partial class Menu : Form
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Int32 vKey);

        private readonly Mem mem = new Mem();
        public Menu()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int pID = mem.GetProcIdFromName("Mordhau-Win64-Shipping");
            if (pID != 0)
            {
                mem.OpenProcess(pID);
                pIDLabel.Text = "MORDHAU PID: " + pID.ToString();

                // 🟢 FIX 1: Force-enable the checkbox layout programmatically on startup
                this.Autoblock.Enabled = true;

                // 🟢 FIX 2: Lower timer intervals so your mouse clicks register cleanly
                this.Global.Interval = 50;
                this.AutoblockTimer.Interval = 50;

                // 🟢 FIX 3: Dynamic branding change 
                this.label15.Text = "made by dark";
            }
            else
            {
                DialogResult res = MessageBox.Show("Mordhau must be running", "Silly window for silly people", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (res == DialogResult.OK)
                {
                    System.Windows.Forms.Application.Exit();
                }
            }
        }

        bool skip = false;
        float defaultfov = 0;

        float WorldDistance(Vector3 Player, Vector3 Enemy)
        {
            return (float)Math.Sqrt(Math.Pow(Player.X - Enemy.X, 2.0) + Math.Pow(Player.Y - Enemy.Y, 2.0) + Math.Pow(Player.Z - Enemy.Z, 2.0));
        }

        private void Global_Tick(object sender, EventArgs e)
        {
            if (mem.ReadByte(Offsets.IsAlive) != 0)
            {
                if (skip == false)
                {
                    float defaultfov = mem.ReadFloat(Offsets.DefaultFOV);
                    skip = true;
                }
                alivelabel.Text = "Alive";

                if (mem.ReadFloat(Offsets.AfkTimer, "") > 170 & (Antiafkbutton.Checked))
                {
                    mem.WriteMemory(Offsets.AfkTimer, "float", "0");
                }

                afklabel.Text = mem.ReadFloat(Offsets.AfkTimer, "").ToString();
                if (mem.ReadByte(Offsets.nextbox, "") == 0) { resuplabel.Text = "Yes"; } else { resuplabel.Text = "No"; }

                if (dodgebutton.Checked)
                {
                    dodgebutton.Enabled = true;
                    mem.WriteMemory(Offsets.CanDodge, "byte", "1");
                    mem.WriteMemory(Offsets.DodgeDuration, "float", (DodgeDurationBar.Value / 100f).ToString());
                    mem.WriteMemory(Offsets.DodgeCooldown, "float", (DodgeCooldownBar.Value / 150f).ToString());
                    dodgecooldown.Text = (DodgeCooldownBar.Value).ToString();
                    dodgetime.Text = (DodgeDurationBar.Value).ToString();
                }
                else
                {
                    mem.WriteMemory(Offsets.CanDodge, "byte", "0");
                    mem.WriteMemory(Offsets.DodgeDuration, "float", (0.35).ToString());
                    mem.WriteMemory(Offsets.DodgeCooldown, "float", (0.15).ToString());
                }

                if (Turncapbutton.Checked)
                {
                    turncapxtrack.Enabled = true;
                    turncapytrack.Enabled = true;
                    label9.Text = turncapxtrack.Value.ToString();
                    label8.Text = turncapytrack.Value.ToString();

                    if (mem.ReadFloat(Offsets.turncapx) != -1)
                    {
                        mem.WriteMemory(Offsets.turncapy, "float", turncapytrack.Value.ToString());
                        mem.WriteMemory(Offsets.turncapx, "float", turncapxtrack.Value.ToString());
                    }
                }

                if (FOVcheck.Checked)
                {
                    FOVtrackbar.Enabled = true;
                    metroLabel2.Text = FOVtrackbar.Value.ToString();
                    mem.WriteMemory(Offsets.RealFOV, "float", FOVtrackbar.Value.ToString());
                }
                else
                {
                    FOVtrackbar.Enabled = false;
                    mem.WriteMemory(Offsets.RealFOV, "float", defaultfov.ToString());
                    metroLabel2.Text = mem.ReadFloat(Offsets.DefaultFOV).ToString();
                }
                if (nosmoke.Checked)
                {
                    nosmoke.Enabled = true;
                    mem.WriteMemory(Offsets.smokesmooth, "float", "0");
                    mem.WriteMemory(Offsets.smokesmoothfield, "float", "0");
                }

                if (ezparry.Checked)
                {
                    nosmoke.Enabled = true;
                    mem.WriteMemory(Offsets.easyparry, "float", "0");
                }

                if (teamcolors.Checked)
                {
                    float MapToFloat(int number)
                    {
                        return (float)number / 255.0F;
                    }

                    mem.WriteMemory(Offsets.folorteamA_R, "float", MapToFloat(Convert.ToInt32(team1R.Value)).ToString());
                    mem.WriteMemory(Offsets.folorteamA_G, "float", MapToFloat(Convert.ToInt32(team1G.Value)).ToString());
                    mem.WriteMemory(Offsets.folorteamA_B, "float", MapToFloat(Convert.ToInt32(team1B.Value)).ToString());
                    team1label.ForeColor = Color.FromArgb(255, Convert.ToInt32(team1R.Value), Convert.ToInt32(team1G.Value), Convert.ToInt32(team1B.Value));

                    mem.WriteMemory(Offsets.folorteamB_R, "float", MapToFloat(Convert.ToInt32(team2R.Value)).ToString());
                    mem.WriteMemory(Offsets.folorteamB_G, "float", MapToFloat(Convert.ToInt32(team2G.Value)).ToString());
                    mem.WriteMemory(Offsets.folorteamB_B, "float", MapToFloat(Convert.ToInt32(team2B.Value)).ToString());
                    team2label.ForeColor = Color.FromArgb(0, Convert.ToInt32(team2R.Value), Convert.ToInt32(team2G.Value), Convert.ToInt32(team2B.Value));
                }
                else { mem.WriteMemory(Offsets.forcecoloroverride, "byte", "0"); }

                if (breakanims.Checked)
                {
                    int keybind = (int)breakanimkeybind.Value;
                    short keyStatus = GetAsyncKeyState((int)keybind);
                    if (keyStatus < 0) { mem.WriteMemory(Offsets.EndTime, "float", "0"); breakanimlabel.Text = "HELD"; breakanimlabel.ForeColor = Color.GreenYellow; }
                    else
                    {
                        breakanimlabel.Text = "UNHELD";
                        breakanimlabel.ForeColor = Color.PaleVioletRed;
                    }
                }
                else { breakanimlabel.ForeColor = Color.PaleVioletRed; breakanimlabel.Text = "OFF"; }
            }
            else { alivelabel.Text = "Dead!"; }
        }

        private Overlay overlay = new Overlay();

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                this.overlay.Show();
                return;
            }
            this.overlay.Hide();
        }

        private void Autoblock_Tick(object sender, EventArgs e)
        {
            if (Autoblock.Checked)
            { 
                int offset = 0;
                int Playeroffset = this.mem.ReadInt(Offsets.PlayerSizeOffset, "") - 1;
                string Player = Offsets.Player_Array + ",0";
                Vector3 PlayerLocation;
                PlayerLocation.X = mem.ReadFloat(Player + ",280," + Offsets.player_x, "", true);
                PlayerLocation.Y = mem.ReadFloat(Player + ",280," + Offsets.player_y, "", true);
                PlayerLocation.Z = mem.ReadFloat(Player + ",280," + Offsets.player_z, "", true);
                Debug.WriteLine(PlayerLocation);

                // 🟢 Executing right-click automation loop target
                InputSimulator.SimulateRightClick();
            }
        }
    }

    // 🟢 Separated standalone hardware helper class
    public class InputSimulator
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type; 
            public MOUSEKEYBDSTRUCT u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct MOUSEKEYBDSTRUCT
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const uint INPUT_MOUSE = 0;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

        public static void SimulateRightClick()
        {
            INPUT[] inputs = new INPUT[2];

            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                u = new MOUSEKEYBDSTRUCT { mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_RIGHTDOWN } }
            };

            inputs[1] = new INPUT
            {
                type = INPUT_MOUSE,
