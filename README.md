Faktureringssystem

Ett modernt faktureringssystem utvecklat i ASP.NET Core med Entity Framework Core och Azure SQL Database. Systemet gör det möjligt att hantera kunder, produkter och fakturor samt generera professionella PDF-fakturor automatiskt.

Funktioner
Hantering av kunder (CRUD)
Hantering av produkter (CRUD)
Skapa och uppdatera fakturor
Automatisk beräkning av moms och totalsummor
PDF-generering med QuestPDF
Azure SQL-databas
Swagger/OpenAPI för testning och dokumentation
Integrationstester för API och affärslogik
Teknisk arkitektur

Projektet är uppbyggt enligt en lagerbaserad arkitektur:

Domain – Entities och modeller
Data – Entity Framework Core och databaskonfiguration
Business – Services och affärslogik
WebApi – API-endpoints och frontend
Tekniker
ASP.NET Core
Entity Framework Core
Azure SQL Database
QuestPDF
Swagger / OpenAPI
xUnit
C#
Fakturaflöde
Skapa kund
Skapa produkter
Skapa faktura
Systemet hämtar aktuella produktpriser
Moms och totalsumma beräknas automatiskt
Fakturan sparas i databasen
PDF-faktura genereras och kan laddas ner
Projektets syfte

Syftet med projektet var att bygga ett komplett backend-system för fakturahantering med fokus på ren arkitektur, affärslogik, databashantering, PDF-generering och molndrift i Azure.

Framtida utveckling
Autentisering och användarhantering
Automatiska fakturanummer
E-postutskick av fakturor
Betalstatus (Paid / Unpaid)
Rollbaserad behörighet
Stöd för flera företag (SaaS)
