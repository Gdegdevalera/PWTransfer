
set xact_abort on
set nocount on

begin tran


-- генерим скрипты для создания индексов. Взято отсюда https://social.technet.microsoft.com/wiki/contents/articles/2958.script-to-create-all-foreign-keys.aspx

DECLARE @sql nvarchar(max)
declare @createIndexScripts table (sql varchar(max))

DECLARE @schema_name sysname; 

DECLARE @table_name sysname; 

DECLARE @constraint_name sysname; 

DECLARE @constraint_object_id int; 

DECLARE @referenced_object_name sysname; 

DECLARE @is_disabled bit; 

DECLARE @is_not_for_replication bit; 

DECLARE @is_not_trusted bit; 

DECLARE @delete_referential_action tinyint; 

DECLARE @update_referential_action tinyint; 

DECLARE @tsql nvarchar(4000); 

DECLARE @tsql2 nvarchar(4000); 

DECLARE @fkCol sysname; 

DECLARE @pkCol sysname; 

DECLARE @col1 bit; 

DECLARE @referenced_schema_name sysname;

 

DECLARE FKcursor CURSOR FOR
     select OBJECT_SCHEMA_NAME(parent_object_id)
         , OBJECT_NAME(parent_object_id), name, OBJECT_NAME(referenced_object_id)
         , object_id
         , is_disabled, is_not_for_replication, is_not_trusted
         , delete_referential_action, update_referential_action, OBJECT_SCHEMA_NAME(referenced_object_id)
    from sys.foreign_keys
    order by 1,2;
OPEN FKcursor;

FETCH NEXT FROM FKcursor INTO @schema_name, @table_name, @constraint_name
    , @referenced_object_name, @constraint_object_id
    , @is_disabled, @is_not_for_replication, @is_not_trusted
    , @delete_referential_action, @update_referential_action, @referenced_schema_name;
WHILE @@FETCH_STATUS = 0

BEGIN
        BEGIN
        SET @tsql = 'ALTER TABLE '
                  + QUOTENAME(@schema_name) + '.' + QUOTENAME(@table_name)
                  + CASE @is_not_trusted
                        WHEN 0 THEN ' WITH CHECK '
                        ELSE ' WITH NOCHECK '
                    END
                  + ' ADD CONSTRAINT ' + QUOTENAME(@constraint_name)
                  + ' FOREIGN KEY (';
        SET @tsql2 = '';
        DECLARE ColumnCursor CURSOR FOR
            select COL_NAME(fk.parent_object_id, fkc.parent_column_id)
                 , COL_NAME(fk.referenced_object_id, fkc.referenced_column_id)
            from sys.foreign_keys fk
            inner join sys.foreign_key_columns fkc
            on fk.object_id = fkc.constraint_object_id
            where fkc.constraint_object_id = @constraint_object_id
            order by fkc.constraint_column_id;
        OPEN ColumnCursor;
        SET @col1 = 1;

        FETCH NEXT FROM ColumnCursor INTO @fkCol, @pkCol;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF (@col1 = 1)
                SET @col1 = 0;
            ELSE
            BEGIN
                SET @tsql = @tsql + ',';
                SET @tsql2 = @tsql2 + ',';
            END;
            SET @tsql = @tsql + QUOTENAME(@fkCol);
            SET @tsql2 = @tsql2 + QUOTENAME(@pkCol);
            FETCH NEXT FROM ColumnCursor INTO @fkCol, @pkCol;
        END;
        CLOSE ColumnCursor;
        DEALLOCATE ColumnCursor;
       SET @tsql = @tsql + ' ) REFERENCES ' + QUOTENAME(@referenced_schema_name) + '.' + QUOTENAME(@referenced_object_name)
                  + ' (' + @tsql2 + ')';
        SET @tsql = @tsql
                  + ' ON UPDATE ' + CASE @update_referential_action
                                        WHEN 0 THEN 'NO ACTION '
                                        WHEN 1 THEN 'CASCADE '
                                        WHEN 2 THEN 'SET NULL '
                                        ELSE 'SET DEFAULT '
                                    END
                  + ' ON DELETE ' + CASE @delete_referential_action
                                        WHEN 0 THEN 'NO ACTION '
                                        WHEN 1 THEN 'CASCADE '
                                        WHEN 2 THEN 'SET NULL '
                                        ELSE 'SET DEFAULT '
                                    END
                 + CASE @is_not_for_replication
                        WHEN 1 THEN ' NOT FOR REPLICATION '
                        ELSE ''
                    END
                  + ';';
        END;
    --PRINT @tsql;
insert into @createIndexScripts values(@tsql)
    FETCH NEXT FROM FKcursor INTO @schema_name, @table_name, @constraint_name
        , @referenced_object_name, @constraint_object_id
        , @is_disabled, @is_not_for_replication, @is_not_trusted
        , @delete_referential_action, @update_referential_action, @referenced_schema_name;
END;
CLOSE FKcursor;
DEALLOCATE FKcursor;


-- удаляем индексы

DECLARE tableCursor CURSOR FOR
	select 'alter table ' + schema_name(Schema_id)+'.'+ object_name(parent_object_id) + '  DROP CONSTRAINT  ' +  QUOTENAME(name) from sys.foreign_keys
	
OPEN tableCursor
FETCH NEXT FROM tableCursor INTO @sql
WHILE @@fetch_status = 0
BEGIN
	PRINT @sql
    EXEC(@sql)
    FETCH NEXT FROM tableCursor INTO @sql
END
CLOSE tableCursor

DEALLOCATE tableCursor


-- удаляем данные в таблицах

DECLARE tableCursor CURSOR FOR
	SELECT 'TRUNCATE TABLE ' + Name FROM sys.tables
		WHERE Name <> '__EFMigrationsHistory'
	
OPEN tableCursor
FETCH NEXT FROM tableCursor INTO @sql
WHILE @@fetch_status = 0
BEGIN
	PRINT @sql
    EXEC(@sql)
    FETCH NEXT FROM tableCursor INTO @sql
END
CLOSE tableCursor

DEALLOCATE tableCursor

-- заново создаем индексы

DECLARE createIndexCursor CURSOR FOR 
select * from @createIndexScripts

OPEN createIndexCursor
FETCH NEXT FROM createIndexCursor INTO @sql
WHILE @@fetch_status = 0
BEGIN
	PRINT @sql
    EXEC(@sql)
    FETCH NEXT FROM createIndexCursor INTO @sql
END
CLOSE createIndexCursor

DEALLOCATE createIndexCursor

--rollback
commit