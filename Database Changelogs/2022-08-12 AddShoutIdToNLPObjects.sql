﻿BEGIN TRANSACTION;
GO

ALTER TABLE [Token] ADD [ShoutId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
GO

ALTER TABLE [Sentiment] ADD [ShoutId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
GO

ALTER TABLE [NamedEntity] ADD [ShoutId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220812101652_AddedShoutIdToNLPObjects', N'5.0.10');
GO

COMMIT;
GO

