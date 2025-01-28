INSERT INTO "Users" ("Id", "Dni", "Password", "Role", "CreatedAt", "UpdatedAt", "IsDeleted")
VALUES
    (
        '3G_a9BexJQ',
        '70919049K',
        '$2a$12$01b4milfHK4MnnHvPC3g2eup3RfV.sF785C25/DTrwoI7eQXJbbHG',
        2,
        NOW(),
        NOW(),
        FALSE
    );
    
INSERT INTO "Users" ("Id", "Dni", "Password", "Role", "CreatedAt", "UpdatedAt", "IsDeleted")
VALUES
    (
    'QEm-6JC1643',
    '50378910D',
    '$2a$12$01b4milfHK4MnnHvPC3g2eup3RfV.sF785C25/DTrwoI7eQXJbbHG',
    0,
    NOW(),
    NOW(),
    FALSE
    );

INSERT INTO "Users" ("Id", "Dni", "Password", "Role", "CreatedAt", "UpdatedAt", "IsDeleted")
VALUES
    (
        'HEm-6JC1644',
        '56780182J',
        '$2a$12$01b4milfHK4MnnHvPC3g2eup3RfV.sF785C25/DTrwoI7eQXJbbHG',
        1,
        NOW(),
        NOW(),
        FALSE
    );

INSERT INTO "Clients" (
    "Id",
    "UserId",
    "FullName",
    "Adress",
    "Photo",
    "PhotoDni",
    "AccountsIds",
    "CreatedAt",
    "UpdatedAt",
    "IsDeleted"
) VALUES (
             'eJTX-_GjsQ', 
             'HEm-6JC1644',            
             'José María Giménez',                  
             'Metropolitano, Madrid',   
             'defaultId.png',             
             'default.png',              
             '{"account1","account2"}',   
             NOW(),                       
             NOW(),                      
             false                        
         );

INSERT INTO "Products" (
    "Id",
    "Name",
    "ProductType",
    "CreatedAt",
    "UpdatedAt",
    "IsDeleted"
) VALUES (
             'z-nrYGKQjuM',
             'Black Card',
             1,
             NOW(),
             NOW(),
             false
         );
             
INSERT INTO "Products" (
    "Id",
    "Name",
    "ProductType",
    "CreatedAt",
    "UpdatedAt",
    "IsDeleted"
) VALUES (
    'Z-QnRlm7XQ', 
    'Savings Account', 
    0, 
    NOW(), 
    NOW(), 
    false
);

INSERT INTO "BankAccounts" (
    "Id",
    "ProductId",
    "ClientId",
    "TarjetaId",
    "IBAN",
    "Balance",
    "AccountType",
    "CreatedAt",
    "UpdatedAt",
    "IsDeleted"
) VALUES (
             'mT_ynBQklw', 
             'Z-QnRlm7XQ',          
             'eJTX-_GjsQ',
             'gCJCp7lRW4Q',                         
             'ES7620770024003102575766',   
             0.00,                        
             0,                     
             NOW(),                        
             NOW(),                       
             false                         
         );

INSERT INTO "CreditCards" (
    "Id", "AccountId", "CardNumber", "Pin", "Cvc", "ExpirationDate", "CreatedAt", "UpdatedAt", "IsDeleted"
) VALUES (
             'gCJCp7lRW4Q', -- ID generado
             'mT_ynBQklw', -- AccountId asociado
             '1234567812345678', -- Número de tarjeta
             '1234',             -- PIN
             '123',              -- CVC
             '2028-12-31',       -- Fecha de expiración
             NOW(),              -- Fecha de creación
             NOW(),              -- Fecha de actualización
             FALSE               -- Marcador de eliminación
         );


 