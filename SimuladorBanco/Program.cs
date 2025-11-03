
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimuladorBanco
{
    public class Cliente
    {
        public string Id { get; private set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }

        public Cliente(string id, string nombre, string email, string telefono)
        {
            Id = id;
            Nombre = nombre;
            Email = email;
            Telefono = telefono;
        }

        public override string ToString()
        {
            return $"Cliente: {Nombre} (ID: {Id})";
        }
    }

    public abstract class Cuenta
    {
        public string Numero { get; protected set; }
        public decimal Saldo { get; protected set; }
        public Cliente Cliente { get; set; }
        public string Tipo { get; protected set; }

        protected Cuenta(string numero, Cliente cliente, string tipo)
        {
            Numero = numero;
            Cliente = cliente;
            Tipo = tipo;
            Saldo = 0;
        }

        public abstract void Depositar(decimal monto);
        public abstract bool Retirar(decimal monto);
        
        public virtual decimal ConsultarSaldo()
        {
            return Saldo;
        }

        public virtual string ObtenerInformacion()
        {
            return $"Cuenta {Tipo} No: {Numero} - Saldo: ${Saldo:N2}";
        }
    }

    public class CuentaAhorros : Cuenta
    {
        public decimal TasaInteres { get; private set; }

        public CuentaAhorros(string numero, Cliente cliente, decimal tasaInteres = 0.02m) 
            : base(numero, cliente, "Ahorros")
        {
            TasaInteres = tasaInteres;
        }

        public override void Depositar(decimal monto)
        {
            if (monto > 0)
            {
                Saldo += monto;
                Console.WriteLine($"✓ Depósito exitoso: ${monto:N2}");
            }
            else
            {
                Console.WriteLine("✗ El monto debe ser positivo");
            }
        }

        public override bool Retirar(decimal monto)
        {
            if (monto > 0 && monto <= Saldo)
            {
                Saldo -= monto;
                Console.WriteLine($"✓ Retiro exitoso: ${monto:N2}");
                return true;
            }
            else
            {
                Console.WriteLine("✗ Fondos insuficientes");
                return false;
            }
        }

        public void CalcularInteres()
        {
            decimal interes = Saldo * TasaInteres;
            Saldo += interes;
            Console.WriteLine($"✓ Interés aplicado: ${interes:N2}");
        }
    }

    public class CuentaCorriente : Cuenta
    {
        public decimal LimiteSobregiro { get; private set; }
        public decimal SobregiroUtilizado { get; private set; }

        public CuentaCorriente(string numero, Cliente cliente, decimal limiteSobregiro = 1000) 
            : base(numero, cliente, "Corriente")
        {
            LimiteSobregiro = limiteSobregiro;
            SobregiroUtilizado = 0;
        }

        public override void Depositar(decimal monto)
        {
            if (monto > 0)
            {
                if (SobregiroUtilizado > 0)
                {
                    decimal paraSobregiro = Math.Min(monto, SobregiroUtilizado);
                    SobregiroUtilizado -= paraSobregiro;
                    monto -= paraSobregiro;
                    Console.WriteLine($"✓ Sobregiro cubierto: ${paraSobregiro:N2}");
                }
                
                if (monto > 0)
                {
                    Saldo += monto;
                }
                
                Console.WriteLine($"✓ Depósito exitoso - Saldo: ${Saldo:N2}");
            }
            else
            {
                Console.WriteLine("✗ Monto inválido");
            }
        }

        public override bool Retirar(decimal monto)
        {
            if (monto <= 0)
            {
                Console.WriteLine("✗ Monto debe ser positivo");
                return false;
            }

            decimal disponibleTotal = Saldo + (LimiteSobregiro - SobregiroUtilizado);
            
            if (monto <= disponibleTotal)
            {
                if (monto <= Saldo)
                {
                    Saldo -= monto;
                }
                else
                {
                    decimal faltante = monto - Saldo;
                    Saldo = 0;
                    SobregiroUtilizado += faltante;
                    Console.WriteLine($"⚠ Usando sobregiro: ${faltante:N2}");
                }
                
                Console.WriteLine($"✓ Retiro exitoso: ${monto:N2}");
                return true;
            }
            else
            {
                Console.WriteLine("✗ Fondos insuficientes incluso con sobregiro");
                return false;
            }
        }
    }

    public class Banco
    {
        public string Nombre { get; private set; }
        private List<Cliente> clientes;
        private List<Cuenta> cuentas;
        private int siguienteNumeroCuenta;

        public Banco(string nombre)
        {
            Nombre = nombre;
            clientes = new List<Cliente>();
            cuentas = new List<Cuenta>();
            siguienteNumeroCuenta = 1001;
        }

        public void AgregarCliente(Cliente cliente)
        {
            clientes.Add(cliente);
            Console.WriteLine($"✓ Cliente {cliente.Nombre} agregado.");
        }

        public Cuenta CrearCuenta(string tipo, Cliente cliente)
        {
            string numeroCuenta = siguienteNumeroCuenta.ToString();
            siguienteNumeroCuenta++;

            Cuenta nuevaCuenta = tipo.ToLower() switch
            {
                "ahorros" => new CuentaAhorros(numeroCuenta, cliente),
                "corriente" => new CuentaCorriente(numeroCuenta, cliente),
                _ => throw new ArgumentException("Tipo no válido")
            };

            cuentas.Add(nuevaCuenta);
            Console.WriteLine($"✓ Cuenta {tipo} #{numeroCuenta} creada para {cliente.Nombre}");
            return nuevaCuenta;
        }

        public void ListarClientes()
        {
            Console.WriteLine($"\n📋 Clientes de {Nombre}");
            Console.WriteLine("----------------------------");
            foreach (var cliente in clientes)
            {
                Console.WriteLine($"  {cliente.ToString()}");
            }
        }

        public void ListarCuentas()
        {
            Console.WriteLine($"\n💳 Cuentas de {Nombre}");
            Console.WriteLine("----------------------------");
            foreach (var cuenta in cuentas)
            {
                Console.WriteLine($"  {cuenta.ObtenerInformacion()}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("🏦 === SIMULADOR DE BANCO ===");
            
            // Crear banco
            Banco miBanco = new Banco("Banco del Pichuncha");
            
            // Crear clientes
            Cliente cliente1 = new Cliente("C001", "Juan Pérez", "juan@email.com", "555-1234");
            Cliente cliente2 = new Cliente("C002", "María García", "maria@email.com", "555-5678");
            
            // Agregar clientes
            miBanco.AgregarCliente(cliente1);
            miBanco.AgregarCliente(cliente2);
            
            // Crear cuentas
            Cuenta cuenta1 = miBanco.CrearCuenta("ahorros", cliente1);
            Cuenta cuenta2 = miBanco.CrearCuenta("corriente", cliente2);
            
            Console.WriteLine("\n💸 Realizando operaciones...");
            Console.WriteLine("=============================");
            
            // Operaciones de prueba
            cuenta1.Depositar(1000);
            cuenta1.Retirar(200);
            cuenta2.Depositar(500);
            cuenta2.Retirar(600); // Probando sobregiro
            cuenta2.Depositar(200);
            
            // Mostrar información final
            miBanco.ListarClientes();
            miBanco.ListarCuentas();
            
            Console.WriteLine("\n🎉 Simulación completada!");
            Console.WriteLine("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
