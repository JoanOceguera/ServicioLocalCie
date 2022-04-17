using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ServicioLocalCie
{
    public partial class ServicioLocalCie : ServiceBase
    {
        private Timer Tiempo;
        private string estado = "BIEN";
        private bool apagado = false;
        private DateTime ultimo;
        LectorHuellasEntities lectorData = new LectorHuellasEntities();
        //BD2Entities bd2data = new BD2Entities();
        

        public ServicioLocalCie()
        {
            Tiempo = new Timer(30000);//esto es cada 1/2 minuto
            Tiempo.Elapsed += new ElapsedEventHandler(Funciones);//aqui asigno al evento "Funciones" el tick del reloj
        }

        public DateTime Ultimo
        {
            get { return ultimo; }
            set { ultimo = value; }
        }

        protected override void OnStart(string[] args)
        {
            //EnviarCorreo("Se ha iniciado el servicio MLector");
            Tiempo.Start();
        }

        protected override void OnStop()
        {
            Tiempo.Stop();
        }
        protected override void OnShutdown()
        {
            base.OnShutdown();
            //EnviarCorreo("Se ha Apagado el servidor de servicios ");
            apagado = true;
        }

        private void Funciones(object sender, ElapsedEventArgs e)
        {
            try
            {
                 Ejecutar();
            }
            catch (Exception ex)
            {
                CExcepcionesyTrazas.ExcepcionyTraza(ex);
            }
        }

        /// <summary>
        /// Metodo recursivo que devuelve la ultima de las trazas 
        /// </summary>
        /// <param name="fecha"> RECIBE UN DATETIME FECHA CON EL DÍA QUE SE QUIERE BUSCAR</param>
        /// <returns>DEVUELVE LA FECHA EXACTA DE LA ÚLTIMA TRAZA</returns>
        private DateTime UltimaTraza(DateTime fecha)
        {
            var dia = fecha.Date.ToString("dd/MM/yy");
            var este = lectorData.RegistroES.Where(x => x.Fecha.Substring(0, 8) == dia).OrderBy(x => x.Fecha).Select(x => x.Fecha).ToList();
            //var este = from todos in Conector.InstanciaLH.RegistroES
            //           where todos.Fecha.Substring(0, 8) == dia
            //           orderby todos.Fecha descending
            //           select todos.Fecha;
            if (este.Any())
            {

                return Convert.ToDateTime(este.FirstOrDefault(), new CultureInfo("es-ES"));
            }
            else
            {
                return UltimaTraza(fecha.AddDays(-1));
            }
        }
        private void Ejecutar()//en esta variante se copia sin borrar de temporal con la referencia por la fecha de la 'ultima traza 
        {
            try
            {
                DateTime nuevo = UltimaTraza(DateTime.Now.Date);
                
                var personaorig = lectorData.RegistroES;
                //var personaorig = from todos in Conector.InstanciaBD2.TempRegistroES
                //                  where todos.Fecha >= nuevo
                //                  select todos;
                if (personaorig.Any())
                {
                    foreach (var item in personaorig)
                    {
                        RegistroES regtemp = new RegistroES();
                        regtemp.Exp = item.Exp;
                        regtemp.IP = item.IP;
                        regtemp.Tipo = item.Tipo;
                        //regtemp.Fecha = Conversor(item.Fecha);
                        if (!Comprobar_Error(regtemp))
                        {
                            lectorData.RegistroES.Add(regtemp);
                            //Conector.InstanciaLH.RegistroES.AddObject(regtemp);
                        }
                    }
                    if (apagado)
                    {
                        return;
                    }
                    lectorData.SaveChanges();
                    if (estado == "ERROR")
                    {
                        estado = "BIEN";
                        //EnviarCorreo("se arregló");   //función k envía la hora y trascendencia de la excepción por correo 
                    }
                }
            }

            catch (Exception EXC)
            {
                if (estado == "BIEN")
                {
                    estado = "ERROR";
                    //EnviarCorreo(EXC.ToString());   //función k envía la hora y trascendencia de la excepción por correo
                    Tiempo.Stop();
                    Tiempo.Start();
                }
            }
        }
        private DateTime Convierte(string fecha)
        {
            return Convert.ToDateTime(fecha, CultureInfo.InvariantCulture);
        }
        //private void EnviarCorreo(string message)
        //{

        //    //La cadena "servidor" es el servidor de correo que enviará tu mensaje
        //    string servidor = "192.168.0.19";
        //    string asunto = "Alerta, Error en Servicio";
        //    string receptor = "inforeportes@cie.cu";
        //    string cuerpo = message;
        //    string emisor = "serviciolocalcie@registro.es";
        //    // Crea el mensaje estableciendo quién lo manda y quién lo recibe
        //    MailMessage mensaje = new MailMessage(emisor, receptor, asunto, cuerpo);
        //    SmtpClient cliente = new SmtpClient(servidor);
        //    //Añade credenciales si el servidor lo requiere.
        //    cliente.Credentials = CredentialCache.DefaultNetworkCredentials;
        //    //Aqui lo envia
        //    cliente.Send(mensaje);
        //}
        private string Conversor(DateTime pfecha)
        {
            string resp = "";

            resp += pfecha.ToString("dd");
            resp += "/";
            resp += pfecha.ToString("MM");
            resp += "/";
            resp += pfecha.ToString("yy");
            resp += " ";
            resp += pfecha.ToString("HH");
            resp += ":";
            resp += pfecha.ToString("mm");
            resp += ":";
            resp += pfecha.ToString("ss");

            return resp;
        }
        private bool Comprobar_Error(RegistroES regtemp)
        {
            var regiguales = lectorData.RegistroES.Where(x => x.Exp == regtemp.Exp && x.Fecha == regtemp.Fecha && x.Tipo == regtemp.Tipo);
            //var regiguales = from registros in Conector.InstanciaLH.RegistroES
            //                 where registros.Exp == regtemp.Exp
            //                 && registros.Fecha == regtemp.Fecha && registros.Tipo == regtemp.Tipo
            //                 select registros;
            if (regiguales.Any())
            {
                return true;
            }
            return false;
        }

        public void Iniciar(string[] args)
        {
            OnStart(args);
            Console.ReadLine();
            OnStop();
        }
    }
}
