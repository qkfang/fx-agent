-- Seed mock data for ResearchArticles, Customers, and CustomerPortfolios
-- Run against the FxDatabase after migrations have been applied

-- Clear existing data (order matters for FK constraints)
DELETE FROM TraderSuggestions;
DELETE FROM TraderNewsFeeds;
DELETE FROM TraderRecommendations;
DELETE FROM Traders;

DELETE FROM CustomerHistories;
DELETE FROM CustomerPreferences;
DELETE FROM CustomerPortfolios;
DELETE FROM Customers;

DELETE FROM ResearchPatterns;
DELETE FROM ResearchDrafts;
DELETE FROM ResearchArticles;
-- Reset identity seeds


DBCC CHECKIDENT ('Traders', RESEED, 0);
DBCC CHECKIDENT ('TraderRecommendations', RESEED, 0);
DBCC CHECKIDENT ('TraderNewsFeeds', RESEED, 0);
DBCC CHECKIDENT ('TraderSuggestions', RESEED, 0);

DBCC CHECKIDENT ('Customers', RESEED, 0);
DBCC CHECKIDENT ('CustomerPortfolios', RESEED, 0);
DBCC CHECKIDENT ('CustomerHistories', RESEED, 0);
DBCC CHECKIDENT ('CustomerPreferences', RESEED, 0);

DBCC CHECKIDENT ('ResearchArticles', RESEED, 0);
DBCC CHECKIDENT ('ResearchDrafts', RESEED, 0);
DBCC CHECKIDENT ('ResearchPatterns', RESEED, 0);