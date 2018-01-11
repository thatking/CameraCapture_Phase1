
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CameraTool
{
    public partial class multi_integration : Form
    {
        public int Period = 0x7000;
        public int Width = 0x820;
        public int Count = 8;
        public int gammaratio = 0;
        public bool gammaenable =false;
        private double FLO_HIGH = 53.6;//check the 0x0E and 0x0F reg of C570
        private double peroid_ms = 13.785;
        private double width_ms = 1.000;

        public bool OK_triggered = false;
        public bool TriggerMode_triggered = false;
        public int trigged_mode = 0;

        const int tmp = 1000;

        private int actual_pulses_under_FLO = 0;
        private int minimum_pulse = 3;//default multi-integration times,with period 0x7000(based on 2.08MHz from FPGA)

        public multi_integration()
        {
            InitializeComponent();
            ms_period.Text = peroid_ms.ToString("f3");
            ms_width.Text = width_ms.ToString("f3");
            TriggerMode_Selection.SelectedIndex = 0;
        }

        private void btnMultiOK_Click(object sender, EventArgs e)
        {
            Period = Convert.ToInt32( txtBoxPeriod.Text,16);
            Width =  Convert.ToInt32( TextWidth.Text,16);
            Count = minimum_pulse;// Convert.ToInt32(NumPulse.Text, 16);
            gammaratio = GammaRatio.Value;
            gammaenable = GammaEN.Checked;

            OK_triggered = true;

            this.Hide();
        }

        private void btnMultiCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void txtBoxPeriod_TextChanged(object sender, EventArgs e)
        {
            Period = Convert.ToInt32(txtBoxPeriod.Text, 16);
            Count = Convert.ToInt32(NumPulse.Text, 16);

            actual_pulses_under_FLO = (int)(FLO_HIGH / ((double)Period / 2080));

            minimum_pulse = (actual_pulses_under_FLO >= Count) ? Count : actual_pulses_under_FLO;

            Count = minimum_pulse;

            Console.WriteLine("Period-->Count:" + Count);

            NumPulses_FLO.Text = Convert.ToString(minimum_pulse);

            peroid_ms = Convert.ToDouble(Period * tmp) / 2080000;

            ms_period.Text = peroid_ms.ToString("f3"); //Convert.ToString(Period);

        }

        private void NumPulse_TextChanged(object sender, EventArgs e)
        {
            Period = Convert.ToInt32(txtBoxPeriod.Text, 16);
            Count = Convert.ToInt32(NumPulse.Text, 16);

            actual_pulses_under_FLO = (int)(FLO_HIGH / ((double)Period / 2080));

           minimum_pulse = (actual_pulses_under_FLO >= Count) ? Count : actual_pulses_under_FLO;

           Count = minimum_pulse;

           Console.WriteLine("Pulse Count:" + Count);

            NumPulses_FLO.Text = Convert.ToString(minimum_pulse);
        }


        private void TextWidth_TextChanged(object sender, EventArgs e)
        {
            Width = Convert.ToInt32(TextWidth.Text, 16);

            width_ms = Convert.ToDouble(Width * tmp) / 2080000;

            ms_width.Text = width_ms.ToString("f3");//Convert.ToString(Width);
        }

        private void TriggerMode_Selection_SelectedIndexChanged(object sender, EventArgs e)
        {
            TriggerMode_triggered = true;
            trigged_mode = TriggerMode_Selection.SelectedIndex;
        }

    }
}
