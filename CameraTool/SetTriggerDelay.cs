/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/
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
    public partial class SetTriggerDelay : Form
    {
        public uint DelayTime = 0;

        public SetTriggerDelay()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                DelayTime = (uint)Convert.ToInt32(txtBTriggerDelayTime.Text, 10);
            }
            catch
            {
            }
            finally
            {
                this.Hide();
            }
        }
    }
}
