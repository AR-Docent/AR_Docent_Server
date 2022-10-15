create table product(
	id int not null primary key identity,
	title nvarchar(20) not null,
	name nvarchar(10) not null,
	created_at Datetime not null default current_timestamp,
	content nvarchar(500) not null,
	audio int foreign key references AudioFiles (ID)
);
