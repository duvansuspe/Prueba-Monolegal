// Base de demostración de Monolegal.
// Ejecutar con MongoDB encendido:
//   mongosh "mongodb://localhost:27017" database/monolegal.js
//
// Borra y vuelve a crear las colecciones clientes, facturas y notificaciones.

const base = db.getSiblingDB("monolegal");

const anaId = ObjectId("64b000000000000000000001");
const carlosId = ObjectId("64b000000000000000000002");
const deltaId = ObjectId("64b000000000000000000003");

const hoy = new Date();
hoy.setUTCHours(0, 0, 0, 0);

function dias(cantidad) {
  const fecha = new Date(hoy);
  fecha.setUTCDate(fecha.getUTCDate() + cantidad);
  return fecha;
}

function factura(clienteId, numero, concepto, valor, diasEmision, diasVencimiento, estado) {
  return {
    clienteId: clienteId,
    numero: numero,
    concepto: concepto,
    valor: NumberDecimal(String(valor)),
    fechaEmision: dias(diasEmision),
    fechaVencimiento: dias(diasVencimiento),
    estado: estado
  };
}

base.clientes.drop();
base.facturas.drop();
base.notificaciones.drop();

base.clientes.insertMany([
  {
    _id: anaId,
    nombre: "Ana María López",
    email: "duvansuspe17@gmail.com",
    documento: "1023456789"
  },
  {
    _id: carlosId,
    nombre: "Carlos Gómez Ruiz",
    email: "carlos.gomez@cliente.test",
    documento: "80123456"
  },
  {
    _id: deltaId,
    nombre: "Inversiones Delta S.A.S.",
    email: "cartera@inversionesdelta.test",
    documento: "900123456"
  }
]);

base.facturas.insertMany([
  factura(anaId, "FAC-1001", "Honorarios de asesoría contractual", 1500000, -40, -10, "primerrecordatorio"),
  factura(anaId, "FAC-1002", "Revisión de minuta de compraventa", 850000, -25, -5, "primerrecordatorio"),
  factura(anaId, "FAC-1003", "Consulta societaria", 400000, -60, -30, "pagada"),

  factura(carlosId, "FAC-2001", "Representación en audiencia", 2200000, -50, -20, "segundorecordatorio"),
  factura(carlosId, "FAC-2002", "Estudio de títulos", 960000, -18, -2, "primerrecordatorio"),
  factura(carlosId, "FAC-2003", "Concepto jurídico laboral", 300000, -90, -60, "desactivado"),

  factura(deltaId, "FAC-3001", "Acompañamiento en due diligence", 5400000, -45, -15, "segundorecordatorio"),
  factura(deltaId, "FAC-3002", "Elaboración de reformas estatutarias", 1100000, -35, -8, "segundorecordatorio"),
  factura(deltaId, "FAC-3003", "Honorarios de secretaría jurídica", 750000, -12, 3, "primerrecordatorio")
]);

base.facturas.createIndex({ clienteId: 1, estado: 1 });

print("Base monolegal lista: 3 clientes y 9 facturas.");
