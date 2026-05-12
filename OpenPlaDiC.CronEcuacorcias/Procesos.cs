using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using OpenPlaDiC.Framework;
using OpenPlaDiC.SF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Text;


namespace OpenPlaDiC.CronEcuacorcias
{
    public class Procesos
    {
        public async Task<Response> CargaPreventaAsync(string connectionString, string fechaCargaPreventa = "")
        {

            Stopwatch timeMeasure = new Stopwatch();
            timeMeasure.Start();


            var _sfService = new SFService();

            var _biz = new OpenPlaDiC.BIZ.DataService(new DAL.AppDbContext(connectionString));


            DataTable table = null;
            List<Pedido> list = new List<Pedido>();
            List<ProductosPreventa> listPP = new List<ProductosPreventa>();
            string fechaConsulta = fechaCargaPreventa;
            if (string.IsNullOrEmpty(fechaCargaPreventa))
            {
                fechaConsulta = DateTime.Now.AddHours(-5).ToString("yyyy-MM-dd");

                fechaConsulta = "2026-04-01";

            }

            Console.WriteLine($"Iniciando carga de preventa para la fecha: {fechaConsulta}");

            var resp = await _sfService.AuthSFAsync(
                                    "3MVG9RezSyZYLh2ugNvdnHEjxlo0ip.U4p7wKX5F_0tRkB6VlsDLqEO.pZPhxXvhddQhj9hbyCgdT0ey9LOcE",
                                    "3FA5714B7E3259CEE987D2A7AA657E68B5E283CB6B3DB8FDBB2D6DC941C60B78",
                                    "jose.medina@ecuacorcias.com",
                                    "Milagro2025+...",
                                    "https://login.salesforce.com/services/oauth2/token"
                                    );

            if (resp.IsSuccess)
            {
                var query = "select Id, Name, Latitud__c, Longitud__c, Estado__c, Fecha_hora_pedido__c, " +
                "Vendedor__r.Name, Vendedor__r.Id, Total_cajas_30L__c, Total_de_cajas__c, " +
                "Total_de_unidades__c, Modulo__r.Name, Modulo__r.Id, Monto_pedido__c,Cliente__c, Cliente__r.Name, " +
                "Codigo_Cliente__c, Cliente__r.DNI_RUC_CE_RFC__c, Latitud_cliente__c, Longitud_cliente__c, " +
                "Visita__r.Avance_del_dia__r.Total_clientes__c, Procedimiento__c, Lista_de_precios__r.Id, Lista_de_precios__r.Name  " +
                "from Pedido__c where Sucursal__c = 'a0pf2000004a2P5AAI' AND fecha__c =  " + fechaConsulta;
                var dataResp = await _sfService.GetSOQLAsync(query, resp.Data);
                if (dataResp.IsSuccess)
                {
                    table = dataResp.Data;
                    if (table != null)
                    {

                        list = OpenPlaDiC.Framework.Helper.ConvertDataTable<Pedido>(table);
                        Console.WriteLine($"Se encontraron {list.Count} pedidos.");
                        int ix = 1;
                        foreach (var p in list)
                        {
                            _biz.ExecProc("sp_UpsertPedidosPreventa",
                                new GlobalItem("IdPedido", p.Id),
                                new GlobalItem("Name", p.Name),
                                new GlobalItem("Latitud__c", p.Latitud__c),
                                new GlobalItem("Longitud__c", p.Longitud__c),
                                new GlobalItem("Estado__c", p.Estado__c),
                                new GlobalItem("Fecha_hora_pedido__c", p.Fecha_hora_pedido__c.ToString("yyyy-MM-dd HH:mm:ss")),
                                new GlobalItem("Vendedor_Name", p.Vendedor__r_Name),
                                new GlobalItem("Vendedor_Id", p.Vendedor__r_Id),
                                new GlobalItem("Total_cajas_30L__c", p.Total_cajas_30L__c.ToString()),
                                new GlobalItem("Total_de_cajas__c", p.Total_de_cajas__c.ToString()),
                                new GlobalItem("Total_de_unidades__c", p.Total_de_unidades__c.ToString()),
                                new GlobalItem("Modulo_Name", p.Modulo__r_Name),
                                new GlobalItem("Modulo_Id", p.Modulo__r_Id),
                                new GlobalItem("Monto_pedido__c", p.Monto_pedido__c.ToString()),
                                new GlobalItem("Cliente_Name", p.Cliente__r_Name),
                                new GlobalItem("Cliente_Id", p.Cliente__c),
                                new GlobalItem("Total_clientes_dia", p.Avance_del_dia__r_Total_clientes__c.ToString()),
                                new GlobalItem("Codigo_Cliente__c", p.Codigo_Cliente__c),
                                new GlobalItem("DNI_RUC_CE_RFC__c", p.Cliente__r_DNI_RUC_CE_RFC__c),
                                new GlobalItem("Latitud_cliente__c", p.Latitud_cliente__c),
                                new GlobalItem("Longitud_cliente__c", p.Longitud_cliente__c),
                                new GlobalItem("Procedimiento__c", p.Procedimiento__c),
                                new GlobalItem("Lista_de_precios_Id", p.Lista_de_precios__r_Id),
                                new GlobalItem("Lista_de_precios_Name", p.Lista_de_precios__r_Name)

                                );

                            Console.WriteLine($"  Pedido {p.Name} actualizado en la base de datos. ({ix++} de {list.Count})");

                            query = "select id, pedido__c, " +
                                "producto__c, " +
                                "producto__r.Name, " +
                                "producto__r.Unidades_Por_Paquete__c, " +
                                "producto__r.SKU__c, " +
                                "Cajas_total__c, " +
                                "Cajas__c, " +
                                "Unidades__c, " +
                                "Procedimiento__c, " +
                                "Precio_unitario__c, " +
                                "Descuento_monto__c, " +    
                                "Descuento__c, " +
                                "Precio_total__c " +
                                " from " +
                                "Producto_Pedido__c " +
                                "where pedido__c = '" + p.Id + "' and isdeleted = false ";

                            dataResp = await _sfService.GetSOQLAsync(query, resp.Data);
                            if (dataResp.IsSuccess)
                            {
                                var tablePP = dataResp.Data;
                                if (tablePP != null)
                                {
                                    var listPPDetalle = OpenPlaDiC.Framework.Helper.ConvertDataTable<ProductoPedido>(tablePP);
                                    Console.WriteLine($"  Se encontraron {listPPDetalle.Count} productos en el pedido {p.Name}.");
                                    foreach (var pp in listPPDetalle)
                                    {
                                        _biz.ExecProc("sp_UpsertProductosPreventaDetalle",
                                            new GlobalItem("Id", pp.Id),
                                            new GlobalItem("Pedido__c", pp.Pedido__c),
                                            new GlobalItem("Producto__c", pp.Producto__c),
                                            new GlobalItem("Producto__r_name", pp.Producto__r_Name),
                                            new GlobalItem("Producto__r_Unidades_Por_Paquete__c", pp.Producto__r_Unidades_Por_Paquete__c.ToString()),
                                            new GlobalItem("Producto__r_sku__c", pp.Producto__r_SKU__c),
                                            new GlobalItem("Cajas_total__c", pp.Cajas_total__c.ToString()),
                                            new GlobalItem("Cajas__c", pp.Cajas__c.ToString()),
                                            new GlobalItem("Unidades__c", pp.Unidades__c.ToString()),
                                            new GlobalItem("Procedimiento__c", pp.Procedimiento__c),
                                            new GlobalItem("Precio_unitario__c", pp.Precio_unitario__c.ToString()),
                                            new GlobalItem("Descuento_monto__c", pp.Descuento_monto__c.ToString()),
                                            new GlobalItem("Descuento__c", pp.Descuento__c.ToString()),
                                            new GlobalItem("Precio_total__c", pp.Precio_total__c.ToString())
                                            );
                                    }
                                }

                                Console.WriteLine($"  Productos de preventa actualizados en la base de datos.");

                            }

                            Console.WriteLine($"  Pedido {p.Name} procesado.");
                            Console.WriteLine();

                        }


                        Console.WriteLine("Pedidos de preventa actualizados en la base de datos.");


                        query = "select  Producto__c, Producto__r.SKU__c SKU, Producto__r.Name Nombre, sum(Cajas__c) Cajas, sum(Unidades__c) Unidades, sum(Precio_total_plano__c) Total  " +
                        "from Producto_Pedido__c where Pedido__r.Sucursal__c = 'a0pf2000004a2P5AAI' AND Pedido__r.fecha__c = " + fechaConsulta + " and isdeleted = false " +
                        "group by  Producto__c, Producto__r.SKU__c, Producto__r.Name ORDER BY SUM(Cajas__c) DESC ";

                        dataResp = await _sfService.GetSOQLAsync(query, resp.Data);
                        if (dataResp.IsSuccess)
                        {
                            table = dataResp.Data;
                            if (table != null)
                            {

                                listPP = OpenPlaDiC.Framework.Helper.ConvertDataTable<ProductosPreventa>(table);

                                Console.WriteLine($"Se encontraron {listPP.Count} productos de preventa.");

                                foreach (var p in listPP)
                                {
                                    _biz.ExecProc("sp_UpsertProductosPreventa",
                                        new GlobalItem("CodigoProducto", p.SKU),
                                        new GlobalItem("Cajas", (p.Cajas ?? 0).ToString()),
                                        new GlobalItem("Unidades", (p.Unidades ?? 0).ToString()),
                                        new GlobalItem("Total", p.Total.ToString()),
                                        new GlobalItem("Fecha", fechaConsulta)
                                        );

                                }

                                Console.WriteLine("Productos de preventa actualizados en la base de datos.");

                            }

                        }
                    }
                }

            }


            timeMeasure.Stop();
            Console.WriteLine($"Tiempo: {timeMeasure.Elapsed.TotalMinutes} minutos");

            return new Response();

        }
    }



    public class Pedido
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Latitud__c { get; set; }
        public string Longitud__c { get; set; }
        public string Estado__c { get; set; }
        public DateTime Fecha_hora_pedido__c { get; set; }
        public string Vendedor__r_Name { get; set; }
        public string Vendedor__r_Id { get; set; }
        public double Total_cajas_30L__c { get; set; }
        public double Total_de_cajas__c { get; set; }
        public double Total_de_unidades__c { get; set; }
        public string Modulo__r_Name { get; set; }
        public string Modulo__r_Id { get; set; }
        public double Monto_pedido__c { get; set; }
        public string Cliente__r_Name { get; set; }
        public string Cliente__c { get; set; }
        public double Avance_del_dia__r_Total_clientes__c { get; set; }
        public string Codigo_Cliente__c { get; set; }
        public string Cliente__r_DNI_RUC_CE_RFC__c { get; set; }
        public string Latitud_cliente__c { get; set; }
        public string Longitud_cliente__c { get; set; }
        public string Procedimiento__c { get; set; }
        public string Lista_de_precios__r_Id { get; set; }
        public string Lista_de_precios__r_Name { get; set; }

    }

    public class ProductoPedido
    {
        public string Id { get; set; }
        public string Pedido__c { get; set; }
        public string Producto__c { get; set; }
        public string Producto__r_Name { get; set; }
        public double? Producto__r_Unidades_Por_Paquete__c { get; set; }
        public string Producto__r_SKU__c { get; set; }
        public double? Cajas_total__c { get; set; }
        public double? Cajas__c { get; set; }
        public double? Unidades__c { get; set; }
        public string Procedimiento__c { get; set; }
        public double? Precio_unitario__c { get; set; }
        public double? Descuento_monto__c { get; set; }
        public double? Descuento__c { get; set; }
        public double? Precio_total__c { get; set; }


    }

    public class ProductosPreventa
    {
        public string Producto__c { get; set; }
        public string SKU { get; set; }
        public string Nombre { get; set; }
        public double? Cajas { get; set; }
        public double? Unidades { get; set; }
        public double Total { get; set; }
    }


}

