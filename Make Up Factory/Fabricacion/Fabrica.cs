﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excepciones;
using Archivos;
using Productos;
using System.Threading;

namespace Fabricacion
{
   // public delegate void DelegadoFabricar(Producto p);
    public class Fabrica
    {
        private List<Producto> productos;
        private List<Jornada> jornadas;
        private int cantidadTrabajadores;
        private List<Thread> hiloProductos;

        /// <summary>
        /// Constructor por defecto de Fabrica
        /// </summary>
        public Fabrica()
        { 

        }

        /// <summary>
        /// Constructor de fábrica
        /// </summary>
        /// <param name="trabajadores">Cantidad de trabajadores</param>
        public Fabrica(int trabajadores)
        {
            this.productos = new List<Producto>();
            this.jornadas = new List<Jornada>();
            this.cantidadTrabajadores = trabajadores;
            this.hiloProductos = new List<Thread>();
        }

        /// <summary>
        /// Propiedad de lectura y escritura de la lista de Jornadas
        /// </summary>
        public List<Jornada> Jornadas
        {
            get
            {
                return this.jornadas;
            }set
            {
                this.jornadas = value;
            }
        }

        /// <summary>
        /// Propiedad de lectura y escritura delalista de Productos
        /// </summary>
        public List<Producto> Productos
        {
            get
            {
                return this.productos;
            }set
            {
                this.productos = value;
            }
        }

        /// <summary>
        /// Metodo que busca el indice de una jornada de acuerdo a una fecha
        /// </summary>
        /// <param name="fecha">Fecha a buscar</param>
        /// <returns>Jornada encontrada con esa fecha o lanza una excepción si no se encontró</returns>
        public int BuscarIndiceJornadaPorFecha(DateTime fecha)
        {
            int indice = -1;

            for (int i = 0; i < this.jornadas.Count; i++)
            {
                if (jornadas[i].Fecha == fecha)
                {
                    indice = i;
                }
            }

            if (indice == -1)
            {
                throw new NoHayActividadEnEsaFechaException();
            }

            return indice;
        }

        /// <summary>
        /// Método para hacer pedido de una determinada cantidad de unidades de un mismo producto
        /// </summary>
        /// <param name="p">Producto del pedido</param>
        /// <param name="cantidadUnidades">Cantidad de unidades de dicho producto</param>
        public void HacerPedido(Producto p, int cantidadUnidades)
        {
            for (int i = 0; i < cantidadUnidades; i++)
            {
                Producto nuevo = (Producto)p.Clone();
                this.productos.Add(nuevo);
            }
        }

        /// <summary>
        /// Método para hacer pedido de una lista de productos
        /// </summary>
        /// <param name="listaPedido">Lista de productos a agregar al pedido</param>
        public void HacerPedido(List<Producto> listaPedido)
        {
            this.productos.AddRange(listaPedido);
        }

        public static void IniciarFabricacion(Fabrica f)
        {
            foreach(Producto p in f.productos)
            {
                Fabrica.Fabricar(p, f);
            }
        }

        /// <summary>
        /// Sobrecarga de operador + que agrega una jornada a la lista de jornadas
        /// </summary>
        /// <param name="f">Fábrica en la qie se cargará la jornada</param>
        /// <param name="j">Jornada a agregar</param>
        /// <returns>La nueva jornada si no hay ninguna en esa fecha o sino la jornada de esa fecha</returns>
        public static Jornada operator +(Fabrica f, Jornada j)
        {
            if (f != j)
            {
                f.jornadas.Add(j);
            }
            else
            {
                j = f.jornadas[f.BuscarIndiceJornadaPorFecha(j.Fecha)];
            }

            return j;
        }

        /// <summary>
        /// Fabrica el producto pasado por parámetro, si no hay tiempo en la jornada para fabricaarlo agrega otra jornada y el producto queda como pendiente
        /// Si hay tiempo se genera un hilo que va actualizando el estado del producto
        /// </summary>
        /// <param name="p"></param>
        /// <param name="f"></param>
        public static void Fabricar(Producto p, Fabrica f)
        {
            DateTime fecha = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            Jornada j = f + new Jornada(fecha, f.cantidadTrabajadores);
            bool agregarJornada = false;

            if (f.productos.Count > 0)
            {
                p.EstaEnSql = false;
                if (p.EstadoActual == Producto.Estado.Nuevo)
                {
                    if (j + p)
                    {
                        p.Vencimiento = f.AsignarVencimiento();

                        Thread hilo = new Thread(p.ActualizarEstados);
                        f.hiloProductos.Add(hilo);
                        hilo.Start();

                    }
                    else
                    {
                        agregarJornada = true;
                    }
                }
            }
            else
            {
                throw new NoSeCargaronProductosException("Para iniciar la fabricación debe cargar al menos un producto");
            }

            if (agregarJornada)
            {
                f.ActualizarPendientes();
            }

        }

        public static void CerrarFabrica(Fabrica f)
        {
            if (f != null)
            {
                foreach (Thread thread in f.hiloProductos)
                {
                    if(thread != null && thread.IsAlive)
                    {
                        thread.Abort();
                    }

                }
            }
        }

        /// <summary>
        /// Actualiza a los pendientes y los agrega a la lista de una nueva jornada con la fecha del día siguiente
        /// </summary>
        private void ActualizarPendientes()
        {
            DateTime fecha = new DateTime(DateTime.Now.AddDays(1).Year, DateTime.Now.AddDays(1).Month, DateTime.Now.AddDays(1).Day);
            Jornada j = this + new Jornada(fecha,this.cantidadTrabajadores);
            bool seAgrego;

            foreach (Producto item in this.productos)
            {
                if(item.EstadoActual == Producto.Estado.Nuevo)
                {
                    seAgrego = j + item;
                }
            }

            this.GuardarPendientesXml();
        }

        /// <summary>
        /// Informe de la actividad de la fábrica
        /// </summary>
        /// <returns>Cadena con el informe</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Jornada j in this.Jornadas)
            {
                sb.AppendLine(j.InformeResumido());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Calcula vencimiento utilizando una fecha aleatoria
        /// </summary>
        /// <returns>El vencimiento</returns>
        private DateTime AsignarVencimiento()
        {
            return DateTime.Now.AddDays(new Random().Next(3 * 365));
        }

        /// <summary>
        /// Sobrecarga del operador == para ver si una fabrica contiene una jornada
        /// </summary>
        /// <param name="f">Fabrica</param>
        /// <param name="j">Jornada cuya fecha se buscará</param>
        /// <returns>True si ya hay una jornada con esa fecha y sino false</returns>
        public static bool operator ==(Fabrica f, Jornada j)
        {
            if(j != null && f != null)
            {
                foreach (Jornada item in f.jornadas)
                {
                    if (item.Fecha.Day == j.Fecha.Day && item.Fecha.Month == j.Fecha.Month && item.Fecha.Year == j.Fecha.Year)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sobrecarga del operador == para ver si una fabrica contiene una jornada
        /// </summary>
        /// <param name="f">Fabrica</param>
        /// <param name="j">Jornada cuya fecha se buscará</param>
        /// <returns>False si ya hay una jornada con esa fecha y sino true</returns>
        public static bool operator !=(Fabrica f, Jornada j)
        {
            return !(f == j);
        }

        /// <summary>
        /// Guarda los productos pendientes de fabricar en formato XML
        /// </summary>
        /// <returns>Verdadero si se pudieron guardar y sino falso</returns>
        public bool GuardarPendientesXml()
        {
            Xml<string> xml = new Xml<string>();
            string pendientes = string.Empty;

            foreach (Producto item in this.productos)
            {
                if(item.EstadoActual == Producto.Estado.Nuevo)
                {
                    pendientes += item.Informe();
                }
            }

            return xml.Guardar("Pendientes.xml", pendientes);
        }

        /// <summary>
        /// Lee los pendientes que están guardados
        /// </summary>
        /// <returns>Cadena de texto con la información del archivo</returns>
        public string LeerPendientesXml()
        {
            Xml<string> xml = new Xml<string>();

            return xml.Leer("Pendientes.xml");
        }

        /// <summary>
        /// Guarda la actividad de la fánrica de acuerdo a lo pasado por parámetro
        /// </summary>
        /// <param name="info">nfor de la actividad a guardar (puede ser labiales,rímeles o bases)</param>
        /// <returns>True si se pudo guardar y sino false</returns>
        public bool GuardarActividadTxt(string info)
        {
            Texto<string> txtFabrica = new Texto<string>();
            return txtFabrica.Guardar("Actividad de la fábrica.txt", info);
        }
    }
}
