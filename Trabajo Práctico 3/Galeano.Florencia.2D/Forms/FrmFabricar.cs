﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fabricacion;
using Excepciones;

namespace Forms
{
    public partial class FrmFabricar : Form
    {
        private Fabrica fabrica;

        /// <summary>
        /// Constructor del formulario
        /// </summary>
        /// <param name="fabrica"></param>
        public FrmFabricar(Fabrica fabrica)
        {
            InitializeComponent();
            this.fabrica = fabrica;
        }

        /// <summary>
        /// Al apretar este botón se inicia el proceso de fabricación
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFabricaar_Click(object sender, EventArgs e)
        {
            try
            {
                Fabrica.IniciarFabricacion(this.fabrica);
                this.rtbInforme.Text = this.fabrica.ToString();
                if (this.fabrica.Jornadas.Count > 1)
                {
                    MessageBox.Show("Algunos productos no se llegaron a fabricar hoy, pero se agregaron a la planificación de mañana y a un archivo XML", "AVISO", MessageBoxButtons.OK);
                }
            }catch(NoSeCargaronProductosException ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }catch(ArchivoException)
            {
                MessageBox.Show("Quedaron productos pendientes. Hubo un error y no se pueden guardar en un archivo", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Se cambia el color del botón
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFabricaar_MouseMove(object sender, MouseEventArgs e)
        {
            this.ForeColor = Color.FromArgb(182, 149, 192);
        }

        /// <summary>
        /// Se vuelve a cambiar el color del botón
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFabricaar_MouseLeave(object sender, EventArgs e)
        {
            this.ForeColor = Color.FromArgb(255,255,255);
        }

    }
}
