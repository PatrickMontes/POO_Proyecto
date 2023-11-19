--***************--CREAR BD--***************--
create database BDProyectoPOO

use BDProyectoPOO


--***************--ACCESO SISTEMA--***************--
create table Rol(
	IdRol int primary key identity(1,1),
	Descripcion varchar(50)
)

insert into Rol values('Administrador'),
					  ('Cliente')

create table Usuario(
	IdUsuario int primary key identity(1,1),
	Nombre varchar(100),
	Correo varchar(100),
	Clave varchar(50),
	IdRol int references Rol(IdRol)
)

insert into Usuario values('Patrick Alexander', 'pAlexander@gmail.com', '741852963', 1),
						  ('Sebastian Sanca', 'sSanca@gmail.com', '963852741', 2)


--***************--LIBRO--***************--
create table Libro (
   IdLibro int identity(1,1) primary key,
   Título varchar(255),
   Imagen varchar(max),
   Precio decimal(10, 2),
   Stock int
);

create procedure sp_listar_libros
as begin
	select*from Libro
end

create procedure sp_buscar_libro
@IdLibro int
as begin
	select*from Libro
	where IdLibro = @IdLibro
end

create procedure sp_insertar_libros
@Título VARCHAR(255),
@Imagen VARCHAR(max),
@Precio DECIMAL(10, 2),
@Stock INT
as begin
	insert into Libro values(@Título, @Imagen, @Precio, @Stock)
end

create procedure sp_eliminar_libros
@IdLibro int
as begin
	delete Libro where IdLibro = @IdLibro
end

create procedure sp_actualizar_libros
@IdLibro int,
@Título VARCHAR(255),
@Imagen VARCHAR(max),
@Precio DECIMAL(10, 2),
@Stock INT
as begin
	if @Imagen = 'null'
		update Libro set Título = @Título, Precio = @Precio, Stock = @Stock
		where IdLibro = @IdLibro
	else
		update Libro set Título = @Título, Imagen = @Imagen, Precio = @Precio, Stock = @Stock
		where IdLibro = @IdLibro
end

set datetime dmy
--***************--CARRITO COMPRAS--***************--
create table Pedido(
	idPedido varchar(8) primary key,
	fechaPedido datetime default(getdate()),
	dniCliente varchar(8),
	nomCliente varchar(200),
	emailCliente varchar(200),
	telefono varchar(20)
)

create table PedidoDetalle(
	idPedido varchar(8) references Pedido(idPedido),
	idLibro int references Libro(idLibro),
	Precio decimal(10,2),
	Cantidad int
)

create function dbo.fx_autoGenera() 
returns varchar(8)
as
begin
	declare @n int
	declare @aux varchar(8) = ISNULL((Select top 1 idPedido from Pedido Order by 1 desc), '00000000')
	
	Set @n = CAST(@aux as int) + 1

	return REPLICATE('0', 8-LEN(CAST(@n as varchar(8))))+CAST(@n as varchar(8))
end

create procedure sp_pedido_agregar
@idPedido varchar(8) output,
@dniCliente varchar(8),
@nomCliente varchar(150),
@emailCliente varchar(255),
@telefono varchar(20)
as
begin
	set @idPedido = dbo.fx_autoGenerar()
	insert Pedido(idPedido, dniCliente, nomCliente, emailCliente, telefono)
	values(@idPedido, @dniCliente, @nomCliente, @emailCliente, @telefono)
end

CREATE PROCEDURE sp_pedidoDetalle_agregar
    @idPedido VARCHAR(8) output,
    @idLibro INT,
    @Precio DECIMAL(10, 2),
    @Cantidad INT
AS
BEGIN
    DECLARE @StockActual INT;

    -- Obtener la cantidad disponible actual del libro
    SELECT @StockActual = Stock FROM Libro WHERE IdLibro = @idLibro;

    -- Verificar si hay suficiente stock
    IF @StockActual >= @Cantidad
    BEGIN
        -- Agregar el detalle del pedido
        INSERT INTO PedidoDetalle (IdPedido, IdLibro, Precio, Cantidad)
        VALUES (@idPedido, @idLibro, @Precio, @Cantidad);

        -- Actualizar el stock del libro
        UPDATE Libro SET Stock = @StockActual - @Cantidad WHERE IdLibro = @idLibro;
    END
    ELSE
    BEGIN
        -- Manejar el caso en que no hay suficiente stock
        THROW 51000, 'No hay suficiente stock disponible para este libro.', 1;
    END
END
drop procedure sp_pedidoDetalle_agregar

create procedure sp_historialPedidos
as
    SELECT p.idPedido, p.nomCliente, p.fechaPedido, pd.Precio
    FROM Pedido p
    JOIN PedidoDetalle pd ON p.idPedido = pd.idPedido;

