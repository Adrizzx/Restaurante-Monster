// ================================================================
// BASE DE DATOS: RestauranteMonster (MongoDB)
// Restaurante Monster | Grupo G06 | Web Avanzada
// Migrado desde SQL Server a MongoDB
//
// Este script es SOLO para la inicialización/reset de la base de datos.
// Es DESTRUCTIVO: borra todas las colecciones (incluidos platos/bebidas
// agregados desde el panel admin) y las vuelve a poblar con los datos semilla.
// NO lo vuelvas a ejecutar después de haber empezado a usar la app en serio,
// a menos que quieras reiniciar todo desde cero.
//
// Ejecutar con: mongosh --file database/restaurante_monster_mongo.js
// ================================================================

db = db.getSiblingDB('RestauranteMonster');

if (db.roles.countDocuments() > 0 || db.platos.countDocuments() > 0) {
  print('AVISO: la base de datos RestauranteMonster ya tiene datos.');
  print('Este script es destructivo y borraria todo lo agregado desde la app (platos, bebidas, pedidos, etc).');
  print('Si de verdad quieres reiniciar todo desde cero, borra manualmente las colecciones');
  print('o comenta esta validacion en el script antes de ejecutarlo de nuevo.');
  quit(1);
}

db.roles.drop();
db.usuarios.drop();
db.tipos_plato.drop();
db.platos.drop();
db.bebidas.drop();
db.stocks.drop();
db.pedidos.drop();
db.facturas.drop();
db.clientes.drop();
db.contadores.drop();

// ===== ROLES =====
db.roles.insertMany([
  { _id: 1, nombre: 'ADMIN' },
  { _id: 2, nombre: 'MESERO' },
  { _id: 3, nombre: 'CLIENTE' }
]);

// ===== TIPOS DE PLATO =====
db.tipos_plato.insertMany([
  { _id: 1, nombre: 'Entrada' },
  { _id: 2, nombre: 'Sopa' },
  { _id: 3, nombre: 'Plato Fuerte' },
  { _id: 4, nombre: 'Postre' }
]);

// ===== USUARIOS =====
// Contrasenas: Admin#Monster2025 y Mesero#Monster2025
// Hash SHA-256 en hex minusculas, igual que Encriptador.Sha256 (UTF-8) en C#
db.usuarios.insertMany([
  {
    _id: 1, nombre: 'Admin', apellido: 'Monster', correo: 'admin@monster.com',
    contrasena: '71b485690ffd3e0e8fd5d162fc4f6dc5519407df168a5381333db9932bbe6fb8',
    telefono: '0999999999', idRol: 1, estado: 1
  },
  {
    _id: 2, nombre: 'Carlos', apellido: 'Mesero', correo: 'mesero@monster.com',
    contrasena: '8921811f0bd3adf5fae89404ae1f231241501210df68fcaad8ea5c050f5134f3',
    telefono: '0988888888', idRol: 2, estado: 1
  }
]);

// ===== PLATOS =====
// Las imagenes apuntan a wwwroot/resources/productos, que es la carpeta servida
// como estatica por ASP.NET (wwwroot). Los platos sin foto propia usan un generico.
let idPlato = 0;
const platos = [
  // ENTRADAS (idTipo=1)
  ['Empanada de morocho', 'Masa de morocho rellena de carne, cebolla y aji', 1.50, 'resources/productos/empanada-de-morocho.jpg', 1],
  ['Bolon de verde', 'Bola de platano verde con chicharron o queso', 2.00, 'resources/productos/bolon-de-verde.jpg', 1],
  ['Ceviche de camaron', 'Camaron en curtido de limon, cebolla y cilantro', 5.50, 'resources/productos/ceviche-de-camaron.jpg', 1],
  ['Patacones con queso', 'Rodajas de platano verde frito con queso blanco', 2.50, 'resources/productos/patacones-con-queso.jpg', 1],
  ['Llapingachos', 'Tortillas de papa con queso y chorizo', 3.50, 'resources/productos/llapingachos.jpg', 1],
  ['Tostadas con aguacate', 'Rodajas de pan con aguacate y sal prieta', 2.00, 'resources/productos/tostadas-con-aguacate.jpg', 1],
  // SOPAS (idTipo=2)
  ['Caldo de bolas de verde', 'Caldo con bolones rellenos de carne y mani', 4.50, 'resources/productos/caldo-bolas-verde.jpg', 2],
  ['Caldo de gallina criolla', 'Gallina de campo en caldo aromatico con hierbas', 5.00, 'resources/productos/caldo-de-gallina.webp', 2],
  ['Caldo de patas', 'Pata de res con mote, mani y hierbas', 4.00, 'resources/productos/caldo-de-patas.png', 2],
  ['Sopa de lentejas', 'Lentejas con verduras, cuero y costilla', 3.50, 'resources/productos/sopa-de-lentejas.jpg', 2],
  ['Locro de papa', 'Sopa espesa de papa con queso y aguacate', 4.00, 'resources/productos/locro-de-papa.jpg', 2],
  ['Fanesca (temporada)', 'Mezcla de 12 granos con bacalao (Semana Santa)', 6.00, 'resources/productos/fanesca.jpg', 2],
  // PLATOS FUERTES (idTipo=3)
  ['Churrasco', 'Carne a la plancha con arroz, ensalada y papas fritas', 8.50, 'resources/productos/churrasco.jpg', 3],
  ['Apanado', 'Filete de pollo apanado con arroz marinero y ensalada', 7.00, 'resources/productos/apanado.webp', 3],
  ['Arroz marinero', 'Arroz con mariscos, camarones, almejas y calamar', 9.00, 'resources/productos/arroz-marinero.jpg', 3],
  ['Seco de pollo', 'Guiso de pollo en salsa de chicha y especias con arroz', 7.50, 'resources/productos/seco-de-pollo.webp', 3],
  ['Seco de chivo', 'Guiso de chivo con arroz amarillo y maduro frito', 8.00, 'resources/productos/seco-de-chivo.avif', 3],
  ['Fritada', 'Carne de cerdo frita con llapingachos, maiz y maduro', 8.50, 'resources/productos/fritada.avif', 3],
  ['Encebollado de atun', 'Sopa espesa de atun con yuca, cebolla y aji', 5.50, 'resources/productos/encebollado-de-atun.jpg', 3],
  ['Corvina al ajillo', 'Filete de corvina en salsa de ajo y mantequilla', 10.00, 'resources/productos/corvina-al-ajillo.jpg', 3],
  ['Tilapia frita entera', 'Tilapia crujiente con patacones y ensalada', 9.50, 'resources/productos/tilapia-frita.jpg', 3],
  ['Menestra con carne', 'Menestra de lenteja con carne asada y arroz', 7.00, 'resources/productos/menestra-con-carne.jpg', 3],
  // POSTRES (idTipo=4)
  ['Dulce de higos', 'Higos en almibar con queso fresco', 3.00, 'resources/productos/dulce-de-higos.webp', 4],
  ['Espumilla', 'Merengue de guayaba con colores pastel', 1.50, 'resources/productos/espumilla.jpg', 4],
  ['Helado de paila', 'Helado artesanal batido a mano, sabores frutales', 2.50, 'resources/productos/helado-de-paila.jpg', 4],
  ['Tres leches', 'Bizcocho empapado en tres leches, crema encima', 3.50, 'resources/sin-imagen.svg', 4],
  ['Arroz con leche', 'Arroz cremoso con canela y pasas', 2.50, 'resources/sin-imagen.svg', 4],
  ['Cake de chocolate', 'Porcion de pastel de chocolate artesanal', 3.00, 'resources/sin-imagen.svg', 4],
  ['Quimbolito', 'Envuelto dulce de maiz con pasas al vapor', 2.00, 'resources/sin-imagen.svg', 4]
];
const platoDocs = platos.map(([nombre, descripcion, precio, imagen, idTipo]) => ({
  _id: ++idPlato, nombre, descripcion, precio, imagen, idTipo, disponible: true
}));
db.platos.insertMany(platoDocs);

// ===== BEBIDAS =====
let idBebida = 0;
const bebidas = [
  ['Jugo de naranjilla', 'Jugo natural de naranjilla ecuatoriana', 2.00, 'resources/productos/jugo-de-naranja.webp'],
  ['Jugo de maracuya', 'Jugo natural de maracuya con azucar', 2.00, 'resources/productos/jugo-de-maracuya.webp'],
  ['Jugo de tomate de arbol', 'Jugo de tomate de arbol fresco', 2.00, 'resources/productos/jugo-de-tomate-de-arbol.jpg'],
  ['Jugo de guanabana', 'Jugo cremoso de guanabana natural', 2.50, 'resources/productos/jugo-de-guanabana.jpg'],
  ['Colada morada', 'Bebida tradicional de granos y frutas moradas', 2.50, 'resources/productos/colada-morada.jpg'],
  ['Morocho con leche', 'Bebida caliente de morocho molido con leche', 2.00, 'resources/productos/morocho.jpg'],
  ['Canelazo', 'Bebida caliente de canela con panela', 2.00, 'resources/productos/canelazo.webp'],
  ['Agua de Jamaica', 'Infusion fria de flor de jamaica', 1.50, 'resources/productos/te-de-jamaica.jpg'],
  ['Coca-Cola', 'Bebida gaseosa 355ml', 1.50, 'resources/productos/coca-cola.png'],
  ['Agua mineral', 'Agua con o sin gas 500ml', 1.00, 'resources/productos/agua-mineral.jpg'],
  ['Cerveza artesanal Monster', 'Cerveza rubia artesanal local', 3.50, 'resources/productos/cerveza-artesanal.jpg'],
  ['Chicha de jora', 'Chicha tradicional fermentada levemente', 2.00, 'resources/productos/chicha-de-jora.webp']
];
const bebidaDocs = bebidas.map(([nombre, descripcion, precio, imagen]) => ({
  _id: ++idBebida, nombre, descripcion, precio, imagen, disponible: true
}));
db.bebidas.insertMany(bebidaDocs);

// ===== STOCK INICIAL =====
let idStock = 0;
const stockDocs = [
  ...platoDocs.map(p => ({ _id: ++idStock, idPlato: p._id, idBebida: null, cantidad: 50, stockMinimo: 5 })),
  ...bebidaDocs.map(b => ({ _id: ++idStock, idPlato: null, idBebida: b._id, cantidad: 80, stockMinimo: 10 }))
];
db.stocks.insertMany(stockDocs);

// ===== CONTADORES (para IDs autoincrementales desde la app) =====
db.contadores.insertMany([
  { _id: 'usuarios', secuencia: 2 },
  { _id: 'platos', secuencia: idPlato },
  { _id: 'bebidas', secuencia: idBebida },
  { _id: 'stocks', secuencia: idStock },
  { _id: 'tipos_plato', secuencia: 4 },
  { _id: 'pedidos', secuencia: 0 },
  { _id: 'detalles_pedido', secuencia: 0 },
  { _id: 'facturas', secuencia: 0 },
  { _id: 'clientes', secuencia: 0 }
]);

// ===== INDICES =====
db.usuarios.createIndex({ correo: 1 }, { unique: true });
db.platos.createIndex({ idTipo: 1 });
db.stocks.createIndex({ idPlato: 1 });
db.stocks.createIndex({ idBebida: 1 });
db.pedidos.createIndex({ idMesero: 1, fecha: 1 });
db.facturas.createIndex({ idPedido: 1 });
db.clientes.createIndex({ idUsuario: 1 });

print('Base de datos RestauranteMonster (MongoDB) creada y poblada correctamente.');
