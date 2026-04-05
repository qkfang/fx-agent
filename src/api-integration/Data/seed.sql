-- Seed mock data for ResearchArticles, Customers, and CustomerPortfolios
-- Run against the FxDatabase after migrations have been applied

-- Clear existing data (order matters for FK constraints)
DELETE FROM TraderNewsFeeds;
DELETE FROM TraderRecommendations;
DELETE FROM CustomerHistories;
DELETE FROM CustomerPortfolios;
DELETE FROM Traders;
DELETE FROM ResearchPatterns;
DELETE FROM ResearchDrafts;
DELETE FROM Customers;
DELETE FROM ResearchArticles;

-- Reset identity seeds
DBCC CHECKIDENT ('Customers', RESEED, 0);
DBCC CHECKIDENT ('ResearchArticles', RESEED, 0);
DBCC CHECKIDENT ('CustomerPortfolios', RESEED, 0);
DBCC CHECKIDENT ('CustomerHistories', RESEED, 0);
DBCC CHECKIDENT ('Traders', RESEED, 0);
DBCC CHECKIDENT ('TraderRecommendations', RESEED, 0);
DBCC CHECKIDENT ('TraderNewsFeeds', RESEED, 0);
DBCC CHECKIDENT ('ResearchDrafts', RESEED, 0);
DBCC CHECKIDENT ('ResearchPatterns', RESEED, 0);

-------------------------------------------------------
-- Customers (10 records)
-------------------------------------------------------
INSERT INTO Customers (Name, Email, Phone, Company, CreatedAt) VALUES
('Alice Johnson',   'alice.johnson@example.com',   '+1-555-0101', 'Global Trading Co',      '2025-09-15T08:30:00'),
('Bob Smith',       'bob.smith@example.com',       '+1-555-0102', 'Pacific Investments',     '2025-10-02T14:20:00'),
('Carlos Rivera',   'carlos.rivera@example.com',   '+44-20-7946-0201', 'Euro Capital Ltd',   '2025-10-18T09:00:00'),
('Diana Chen',      'diana.chen@example.com',      '+852-3456-7890', 'Asia Macro Fund',      '2025-11-05T11:45:00'),
('Erik Lindberg',   'erik.lindberg@example.com',   '+46-8-123-4567', 'Nordic FX Partners',   '2025-11-20T07:15:00'),
('Fatima Al-Rashid','fatima.alrashid@example.com', '+971-4-555-6789', 'Gulf Finance Group',  '2025-12-01T10:30:00'),
('Grace Park',      'grace.park@example.com',      '+82-2-555-3456', 'Seoul Wealth Mgmt',    '2026-01-10T13:00:00'),
('Hugo Martinez',   'hugo.martinez@example.com',   '+52-55-5555-7890', 'LatAm FX Advisors', '2026-01-25T16:45:00'),
('Isla Thompson',   'isla.thompson@example.com',   '+61-2-5555-1234', 'Oceania Partners',   '2026-02-14T08:00:00'),
('James Okafor',    'james.okafor@example.com',    '+234-1-555-9012', 'Frontier Markets Inc','2026-03-01T12:30:00');

-------------------------------------------------------
-- ResearchArticles (15 records)
-------------------------------------------------------
INSERT INTO ResearchArticles (Title, Summary, Content, Category, Author, PublishedDate, Status, Tags, Sentiment) VALUES
('USD Strength Outlook Q2 2026',
 'Analysis of USD momentum heading into Q2 driven by Fed policy expectations.',
 'The US dollar has shown resilience amid shifting rate expectations. Key drivers include persistent inflation data and labor market tightness. We expect the DXY to trade in the 104-107 range through Q2.',
 'Macro Analysis', 'Sarah Mitchell', '2026-03-28T09:00:00', 'Published', 'USD,Fed,interest-rates,macro', 'Bullish'),

('EUR/USD Technical Levels to Watch',
 'Key support and resistance zones for EUR/USD in the near term.',
 'EUR/USD is consolidating near the 1.0820 level after failing to break above 1.0900. The 200-day moving average at 1.0780 provides critical support. A break below opens the door to 1.0700.',
 'Technical Analysis', 'Marco Rossi', '2026-03-30T10:30:00', 'Published', 'EUR/USD,technical,support,resistance', 'Bearish'),

('GBP Impact from UK Fiscal Policy',
 'How upcoming UK budget decisions may influence the British pound.',
 'The UK government faces challenging fiscal choices that could weigh on sterling. Potential tax increases and spending cuts may slow growth, while the Bank of England remains cautious on rate cuts.',
 'Macro Analysis', 'Emily Carter', '2026-03-25T08:15:00', 'Published', 'GBP,UK,fiscal-policy,BoE', 'Bearish'),

('JPY Carry Trade Dynamics in 2026',
 'Examining whether the yen carry trade remains attractive given BOJ policy shifts.',
 'With the BOJ gradually normalizing policy, the traditional yen carry trade faces headwinds. However, the wide rate differential with the US continues to support short-yen positions for now.',
 'Strategy', 'Kenji Tanaka', '2026-04-01T07:00:00', 'Published', 'JPY,carry-trade,BOJ,rates', 'Neutral'),

('AUD/NZD Cross Rate Opportunity',
 'Relative value opportunity in the Antipodean cross as fundamentals diverge.',
 'Australian economic data has outperformed New Zealand recently, creating a tactical long AUD/NZD opportunity. We target 1.1050 with a stop at 1.0850.',
 'Trade Ideas', 'Liam O''Brien', '2026-04-02T11:00:00', 'Published', 'AUD/NZD,relative-value,Antipodean', 'Bullish'),

('CHF Safe Haven Flows Monitor',
 'Tracking safe haven demand for the Swiss franc amid geopolitical uncertainty.',
 'Geopolitical tensions have periodically boosted CHF demand. We monitor EUR/CHF positioning and SNB intervention signals as key indicators for franc direction.',
 'Macro Analysis', 'Sarah Mitchell', '2026-04-03T09:45:00', 'Published', 'CHF,safe-haven,SNB,geopolitics', 'Neutral'),

('Emerging Market FX Weekly Review',
 'Weekly performance review of major EM currencies against the dollar.',
 'EM currencies had a mixed week. BRL and MXN outperformed on commodity strength, while TRY and ZAR lagged due to domestic policy concerns. Overall EM sentiment remains cautiously optimistic.',
 'Weekly Review', 'Hugo Santos', '2026-04-04T06:30:00', 'Published', 'EM,BRL,MXN,TRY,ZAR,weekly', 'Neutral'),

('CAD and Oil Price Correlation Update',
 'Reassessing the CAD-crude oil relationship as energy markets evolve.',
 'The historical correlation between CAD and WTI oil prices has weakened in 2026. Diversification of Canada''s economy and shifting trade patterns suggest a more nuanced approach to trading USD/CAD.',
 'Correlation Study', 'Emily Carter', '2026-03-20T14:00:00', 'Published', 'CAD,oil,correlation,USD/CAD', 'Neutral'),

('Central Bank Watch: April 2026',
 'Preview of upcoming central bank meetings and expected policy decisions.',
 'April features decisions from the ECB, BOC, and RBA. Markets price a 60% chance of an ECB cut, while the BOC and RBA are expected to hold. We outline scenario analysis for each.',
 'Central Banks', 'Marco Rossi', '2026-04-01T08:00:00', 'Published', 'central-banks,ECB,BOC,RBA,rates', 'Neutral'),

('USD/CNH: Navigating Trade Policy Risks',
 'How evolving trade policies affect the offshore yuan outlook.',
 'Trade policy uncertainty continues to create volatility in USD/CNH. We analyze tariff scenarios and PBOC management of the daily fix as key drivers for the pair.',
 'Geopolitics', 'Kenji Tanaka', '2026-03-27T10:00:00', 'Published', 'CNH,trade-policy,PBOC,tariffs', 'Bearish'),

('Volatility Smile Analysis: Major Pairs',
 'Options market signals for G10 currency pairs.',
 'Risk reversals across major pairs show elevated demand for USD calls vs EUR and GBP puts, signaling hedging activity against further dollar strength. Vol surfaces suggest limited downside risk to USD.',
 'Options & Volatility', 'Sarah Mitchell', '2026-04-03T12:00:00', 'Published', 'volatility,options,risk-reversal,G10', 'Bullish'),

('NOK/SEK Divergence Play',
 'Scandinavian currencies offer a mean-reversion opportunity.',
 'NOK has underperformed SEK by 3% YTD despite similar macro fundamentals. Oil price stabilization and Norges Bank rhetoric support a tactical long NOK/SEK position targeting 0.9850.',
 'Trade Ideas', 'Erik Svensson', '2026-03-22T09:30:00', 'Draft', 'NOK,SEK,Scandinavia,mean-reversion', 'Bullish'),

('Digital Currency Impact on FX Markets',
 'How CBDCs and stablecoin adoption are reshaping foreign exchange flows.',
 'Central bank digital currencies are progressing from pilot to implementation in several economies. We examine how this may affect cross-border payment flows and traditional FX market structure.',
 'Thematic Research', 'Liam O''Brien', '2026-03-18T15:00:00', 'Published', 'CBDC,digital-currency,fintech,structure', 'Neutral'),

('GBP/JPY Breakout Watch',
 'GBP/JPY approaching a multi-month range breakout level.',
 'GBP/JPY has been compressing in a 188.00-192.50 range for eight weeks. Momentum indicators suggest a breakout is imminent. A close above 192.50 targets 196.00; below 188.00 opens 184.50.',
 'Technical Analysis', 'Marco Rossi', '2026-04-04T07:45:00', 'Published', 'GBP/JPY,technical,breakout,range', 'Bullish'),

('Quarterly FX Forecast Update',
 'Updated forecasts for major and select EM currency pairs through Q4 2026.',
 'We revise our EUR/USD forecast lower to 1.0600 by year-end from 1.0900 previously. GBP/USD is adjusted to 1.2400. USD/JPY target raised to 158. Full forecast table included.',
 'Forecast', 'Emily Carter', '2026-04-05T06:00:00', 'Draft', 'forecast,EUR/USD,GBP/USD,USD/JPY,outlook', 'Bearish');

-------------------------------------------------------
-- CustomerPortfolios (20 records)
-------------------------------------------------------
-- Note: Fabric SQL RESEED 0 produces IDs starting at 0, so CustomerId references are 0-based
INSERT INTO CustomerPortfolios (CustomerId, CurrencyPair, Direction, Amount, EntryRate, OpenedAt, Status) VALUES
(0, 'EUR/USD', 'Buy',  100000.0000, 1.082300, '2026-03-10T09:15:00', 'Open'),
(0, 'GBP/USD', 'Sell',  75000.0000, 1.264500, '2026-03-15T14:30:00', 'Open'),
(1, 'USD/JPY', 'Buy',  200000.0000, 151.450000, '2026-02-20T08:00:00', 'Closed'),
(1, 'AUD/USD', 'Buy',   50000.0000, 0.653200, '2026-03-22T10:45:00', 'Open'),
(2, 'EUR/GBP', 'Sell', 150000.0000, 0.856100, '2026-03-01T11:00:00', 'Open'),
(2, 'USD/CHF', 'Buy',  120000.0000, 0.882400, '2026-03-18T07:30:00', 'Closed'),
(3, 'USD/CNH', 'Sell', 300000.0000, 7.245600, '2026-02-28T04:00:00', 'Open'),
(3, 'AUD/NZD', 'Buy',   80000.0000, 1.087500, '2026-04-02T06:15:00', 'Open'),
(4, 'EUR/SEK', 'Sell', 250000.0000, 11.235000, '2026-03-12T08:45:00', 'Closed'),
(4, 'NOK/SEK', 'Buy',  100000.0000, 0.972000, '2026-03-25T09:00:00', 'Open'),
(5, 'USD/AED', 'Buy',  500000.0000, 3.672900, '2026-01-15T10:00:00', 'Closed'),
(5, 'EUR/USD', 'Sell', 200000.0000, 1.078600, '2026-04-01T13:20:00', 'Open'),
(6, 'USD/KRW', 'Sell', 150000.0000, 1345.500000, '2026-03-05T02:30:00', 'Open'),
(6, 'GBP/JPY', 'Buy',  90000.0000, 190.780000, '2026-04-03T05:00:00', 'Open'),
(7, 'USD/MXN', 'Sell', 175000.0000, 17.125000, '2026-02-10T16:00:00', 'Closed'),
(7, 'EUR/USD', 'Buy',  100000.0000, 1.085100, '2026-04-04T09:30:00', 'Open'),
(8, 'AUD/USD', 'Sell',  60000.0000, 0.657800, '2026-03-28T01:00:00', 'Open'),
(8, 'NZD/USD', 'Buy',   45000.0000, 0.598400, '2026-04-01T22:45:00', 'Open'),
(9, 'USD/ZAR', 'Buy', 200000.0000, 18.450000, '2026-03-08T12:00:00', 'Closed'),
(9, 'GBP/USD', 'Buy', 130000.0000, 1.258900, '2026-04-02T14:00:00', 'Open');

-------------------------------------------------------
-- CustomerHistories (12 records — closed trades)
-------------------------------------------------------
INSERT INTO CustomerHistories (CustomerId, CurrencyPair, Direction, Amount, EntryRate, ExitRate, PnL, OpenedAt, ClosedAt, Notes) VALUES
(0, 'EUR/USD', 'Buy',  150000.0000, 1.075000, 1.083200,  1230.0000, '2025-11-10T09:00:00', '2025-12-05T14:30:00', 'Took profit at resistance level'),
(0, 'USD/JPY', 'Sell', 100000.0000, 153.200000, 151.800000, 920.0000, '2025-12-15T08:00:00', '2026-01-10T11:00:00', 'BOJ policy shift benefited position'),
(1, 'GBP/USD', 'Buy',   80000.0000, 1.255000, 1.248000, -560.0000, '2026-01-05T10:30:00', '2026-01-20T16:00:00', 'Stopped out on UK data miss'),
(1, 'USD/JPY', 'Buy',  200000.0000, 149.500000, 151.450000, 2610.0000, '2026-01-25T08:00:00', '2026-02-20T08:00:00', 'Target hit on Fed hawkish pivot'),
(2, 'USD/CHF', 'Buy',  120000.0000, 0.878000, 0.882400, 600.0000, '2026-02-01T07:30:00', '2026-03-18T07:30:00', 'Gradual grind higher on risk-on flows'),
(3, 'AUD/USD', 'Sell',  90000.0000, 0.661000, 0.655200, 522.0000, '2025-10-20T04:00:00', '2025-11-15T06:00:00', 'Commodity weakness drove AUD lower'),
(4, 'EUR/SEK', 'Sell', 250000.0000, 11.320000, 11.235000, 1887.5000, '2026-01-10T08:45:00', '2026-03-12T08:45:00', 'Riksbank rate differential narrowed'),
(5, 'USD/AED', 'Buy',  500000.0000, 3.672500, 3.672900, 200.0000, '2025-11-01T10:00:00', '2026-01-15T10:00:00', 'Peg-related trade with carry'),
(7, 'USD/MXN', 'Sell', 175000.0000, 17.350000, 17.125000, 2268.7500, '2025-12-01T16:00:00', '2026-02-10T16:00:00', 'Nearshoring theme supported MXN'),
(9, 'USD/ZAR', 'Buy',  200000.0000, 18.100000, 18.450000, 3500.0000, '2026-01-08T12:00:00', '2026-03-08T12:00:00', 'Political uncertainty lifted USD/ZAR'),
(3, 'EUR/GBP', 'Buy',  100000.0000, 0.862000, 0.858500, -350.0000, '2025-09-20T09:00:00', '2025-10-15T14:00:00', 'UK outperformance reversed position'),
(6, 'USD/KRW', 'Buy',  120000.0000, 1320.000000, 1338.500000, 1680.0000, '2025-11-05T02:30:00', '2026-01-20T05:00:00', 'EM sell-off drove KRW weaker');

-------------------------------------------------------
-- Traders (8 records)
-------------------------------------------------------
INSERT INTO Traders (Name, Email, Desk, Specialization, Region, IsActive, JoinedAt) VALUES
('Sarah Mitchell',   'sarah.mitchell@example.com',   'G10 Spot',        'USD,EUR,GBP',          'North America', 1, '2020-03-15T09:00:00'),
('Marco Rossi',      'marco.rossi@example.com',      'G10 Options',     'EUR/USD,GBP/JPY',      'Europe',        1, '2019-06-01T08:00:00'),
('Kenji Tanaka',     'kenji.tanaka@example.com',      'Asia FX',         'JPY,CNH,KRW',          'Asia Pacific',  1, '2021-01-10T07:00:00'),
('Emily Carter',     'emily.carter@example.com',      'Macro Strategy',  'G10 Macro,Forecasting', 'Europe',       1, '2018-09-20T08:30:00'),
('Liam O''Brien',    'liam.obrien@example.com',       'EM Trading',      'BRL,MXN,ZAR',          'North America', 1, '2022-04-05T09:00:00'),
('Erik Svensson',    'erik.svensson@example.com',     'Scandinavian FX', 'NOK,SEK,DKK',          'Europe',        1, '2021-07-15T08:00:00'),
('Hugo Santos',      'hugo.santos@example.com',       'EM Research',     'EM Weekly,BRL,TRY',    'Latin America', 1, '2023-02-01T10:00:00'),
('Aisha Patel',      'aisha.patel@example.com',       'G10 Spot',        'AUD,NZD,CAD',          'Asia Pacific',  0, '2020-11-10T07:30:00');

-------------------------------------------------------
-- TraderRecommendations (15 records)
-------------------------------------------------------
INSERT INTO TraderRecommendations (TraderId, CurrencyPair, Direction, TargetRate, StopLoss, Confidence, Rationale, Status, CreatedAt, ExpiresAt) VALUES
(0, 'EUR/USD', 'Sell', 1.065000, 1.092000, 'High',   'DXY momentum and ECB rate cut expectations favor further EUR weakness', 'Active', '2026-04-01T09:00:00', '2026-04-30T23:59:00'),
(0, 'GBP/USD', 'Sell', 1.240000, 1.272000, 'Medium', 'UK fiscal headwinds and relative US strength', 'Active', '2026-04-02T10:00:00', '2026-04-30T23:59:00'),
(1, 'EUR/USD', 'Sell', 1.070000, 1.090000, 'High',   'Options vol surface skewed to USD calls, risk reversals confirm', 'Active', '2026-04-03T08:30:00', '2026-04-18T23:59:00'),
(1, 'GBP/JPY', 'Buy',  196.000000, 187.500000, 'Medium', 'Range breakout imminent above 192.50 with momentum confirmation', 'Active', '2026-04-04T07:45:00', '2026-04-25T23:59:00'),
(2, 'USD/JPY', 'Buy',  158.000000, 149.500000, 'High',   'Wide rate differential persists despite BOJ normalization', 'Active', '2026-04-01T04:00:00', '2026-06-30T23:59:00'),
(2, 'USD/CNH', 'Buy',  7.350000, 7.180000, 'Medium', 'Trade policy uncertainty supports USD premium vs CNH', 'Active', '2026-03-28T05:00:00', '2026-05-15T23:59:00'),
(3, 'EUR/USD', 'Sell', 1.060000, 1.095000, 'High',   'Year-end forecast revision lower to 1.0600, macro deterioration in EZ', 'Active', '2026-04-05T06:00:00', '2026-12-31T23:59:00'),
(3, 'USD/JPY', 'Buy',  158.000000, 148.000000, 'Medium', 'Revised target higher on Fed-BOJ policy divergence', 'Active', '2026-04-05T06:30:00', '2026-12-31T23:59:00'),
(4, 'AUD/NZD', 'Buy',  1.105000, 1.085000, 'Medium', 'AU data outperformance vs NZ supports cross', 'Active', '2026-04-02T11:00:00', '2026-04-20T23:59:00'),
(4, 'USD/MXN', 'Sell', 16.800000, 17.400000, 'Medium', 'Nearshoring theme intact, stable BANXICO policy', 'Expired', '2026-02-15T16:00:00', '2026-03-15T23:59:00'),
(5, 'NOK/SEK', 'Buy',  0.985000, 0.960000, 'Medium', '3pct YTD underperformance vs SEK, mean-reversion with oil support', 'Active', '2026-03-22T09:30:00', '2026-04-30T23:59:00'),
(5, 'EUR/SEK', 'Sell', 11.100000, 11.350000, 'Low',   'Riksbank hawkish tilt could support SEK near term', 'Active', '2026-04-01T08:00:00', '2026-04-20T23:59:00'),
(6, 'USD/ZAR', 'Buy',  19.200000, 17.800000, 'Low',   'Political risk premium expanding ahead of elections', 'Active', '2026-04-04T06:30:00', '2026-05-31T23:59:00'),
(6, 'USD/TRY', 'Buy',  38.500000, 36.000000, 'Low',   'Unorthodox policy risk persists despite recent stabilization', 'Expired', '2026-01-15T10:00:00', '2026-03-01T23:59:00'),
(7, 'AUD/USD', 'Sell', 0.640000, 0.665000, 'Medium', 'China slowdown concerns weigh on AUD beta', 'Closed', '2026-03-01T01:00:00', '2026-03-28T23:59:00');

-------------------------------------------------------
-- TraderNewsFeeds (18 records)
-------------------------------------------------------
INSERT INTO TraderNewsFeeds (TraderId, Headline, Source, Category, CurrencyPairs, Sentiment, Summary, IsRead, PublishedAt) VALUES
(0, 'Fed signals patience on rate cuts amid sticky inflation', 'Central Bank Wire', 'Central Banks', 'USD', 'Bullish', 'Federal Reserve officials reiterated data-dependent approach, pushing back on near-term rate cut expectations.', 1, '2026-04-04T14:00:00'),
(0, 'US payrolls beat expectations with 275K jobs added', 'Economic Data Feed', 'Economic Data', 'USD,EUR/USD,GBP/USD', 'Bullish', 'March NFP surpassed consensus of 200K, with revisions higher. Unemployment rate steady at 3.7%.', 1, '2026-04-04T13:30:00'),
(1, 'ECB Lagarde hints at June rate cut possibility', 'Central Bank Wire', 'Central Banks', 'EUR/USD,EUR/GBP', 'Bearish', 'ECB President signaled growing confidence in disinflation trajectory, opening door for June easing.', 1, '2026-04-03T15:00:00'),
(1, 'GBP/JPY options activity surges near range highs', 'Options Flow Analytics', 'Options & Volatility', 'GBP/JPY', 'Bullish', 'Large call buying in GBP/JPY 193-195 strikes observed, consistent with breakout positioning.', 0, '2026-04-04T08:00:00'),
(2, 'BOJ Ueda confirms gradual normalization path', 'Central Bank Wire', 'Central Banks', 'USD/JPY,JPY', 'Neutral', 'BOJ Governor reiterated cautious approach to rate hikes, maintaining accommodation while monitoring wages.', 1, '2026-04-02T03:00:00'),
(2, 'China PMI data softer than expected in March', 'Economic Data Feed', 'Economic Data', 'USD/CNH,AUD/USD', 'Bearish', 'Manufacturing PMI fell to 49.2 vs 50.1 expected, raising concerns about growth momentum.', 1, '2026-04-01T02:00:00'),
(3, 'IMF revises global growth forecasts lower', 'Macro Research Wire', 'Macro Analysis', 'EUR/USD,GBP/USD,USD/JPY', 'Bearish', 'IMF cut 2026 global GDP forecast to 2.8% from 3.1%, citing trade uncertainty and geopolitical risks.', 1, '2026-04-03T10:00:00'),
(3, 'UK Spring Budget includes spending restraint measures', 'Government & Policy', 'Fiscal Policy', 'GBP,GBP/USD', 'Bearish', 'Chancellor announced GBP 12B in spending cuts and targeted tax increases, weighing on growth outlook.', 0, '2026-04-04T12:00:00'),
(4, 'Brazil central bank holds Selic rate at 11.25%', 'Central Bank Wire', 'Central Banks', 'USD/BRL,BRL', 'Neutral', 'BCB maintained rate as expected, signaling readiness to resume cutting cycle if inflation eases further.', 1, '2026-04-02T18:00:00'),
(4, 'MXN rallies on nearshoring investment surge', 'EM Markets Daily', 'Emerging Markets', 'USD/MXN,MXN', 'Bullish', 'Mexico attracted record FDI in Q1 2026, supporting peso strength and narrowing risk premium.', 1, '2026-04-03T16:00:00'),
(5, 'Norges Bank surprises with hawkish hold', 'Central Bank Wire', 'Central Banks', 'NOK,NOK/SEK,EUR/NOK', 'Bullish', 'Norges Bank held rates but hawkish statement surprised markets, boosting NOK across the board.', 1, '2026-04-03T09:00:00'),
(5, 'Swedish CPI undershoots, Riksbank cut odds rise', 'Economic Data Feed', 'Economic Data', 'SEK,EUR/SEK,NOK/SEK', 'Bearish', 'March Swedish CPI printed 1.8% vs 2.1% expected, increasing probability of Riksbank rate cut.', 0, '2026-04-04T07:30:00'),
(6, 'South Africa load shedding returns to stage 4', 'EM Markets Daily', 'Emerging Markets', 'USD/ZAR,ZAR', 'Bearish', 'Eskom announced return to stage 4 load shedding, dampening ZAR sentiment and growth outlook.', 1, '2026-04-04T10:00:00'),
(6, 'Turkey current account deficit widens sharply', 'Economic Data Feed', 'Economic Data', 'USD/TRY,TRY', 'Bearish', 'February CA deficit came in at -$5.8B vs -$4.2B expected, renewing concerns about external financing.', 0, '2026-04-03T11:00:00'),
(7, 'RBA holds rates, dovish tilt in forward guidance', 'Central Bank Wire', 'Central Banks', 'AUD/USD,AUD', 'Bearish', 'RBA kept rates unchanged but softened language on inflation risks, signaling possible cuts ahead.', 1, '2026-04-01T04:30:00'),
(7, 'NZD supported by dairy auction price surge', 'Commodity Wire', 'Commodities', 'NZD/USD,AUD/NZD', 'Bullish', 'GDT dairy prices jumped 8.2% at latest auction, providing tailwind for NZD.', 0, '2026-04-04T06:00:00'),
(0, 'US CPI inflation holds above 3% for third month', 'Economic Data Feed', 'Economic Data', 'USD,EUR/USD', 'Bullish', 'Core CPI remained elevated at 3.2% YoY, reducing likelihood of near-term Fed rate cuts.', 0, '2026-04-05T08:30:00'),
(3, 'G7 finance ministers discuss currency coordination', 'Government & Policy', 'Geopolitics', 'USD,EUR,JPY,GBP', 'Neutral', 'G7 communique reaffirmed commitment to market-determined exchange rates amid dollar strength concerns.', 0, '2026-04-05T09:00:00');

-------------------------------------------------------
-- ResearchDrafts (8 records)
-------------------------------------------------------
INSERT INTO ResearchDrafts (Title, Content, Author, Category, Tags, Status, Version, ReviewerNotes, CreatedAt, UpdatedAt) VALUES
('EUR/USD H2 2026 Outlook',
 'Draft analysis of EUR/USD trajectory for the second half of 2026. Key themes include ECB policy path, US election influence, and transatlantic growth differential. Initial thesis favors further EUR weakness toward 1.0400.',
 'Emily Carter', 'Forecast', 'EUR/USD,forecast,H2-2026,ECB', 'InProgress', 1, '', '2026-04-04T08:00:00', NULL),

('JPY Intervention Risk Assessment',
 'Analysis of MOF/BOJ intervention thresholds and historical patterns. Examining verbal intervention signals and actual intervention likelihood as USD/JPY approaches 155 level.',
 'Kenji Tanaka', 'Strategy', 'JPY,intervention,MOF,BOJ', 'InProgress', 2, 'Add quantitative analysis of past intervention P&L for context', '2026-04-01T05:00:00', '2026-04-03T09:00:00'),

('EM FX Resilience in Risk-Off Scenarios',
 'Study of EM currency performance during recent risk-off episodes. Compares 2024-2026 drawdowns with historical patterns to assess whether EM FX has become more resilient.',
 'Hugo Santos', 'Thematic Research', 'EM,risk-off,resilience,drawdown', 'UnderReview', 1, 'Consider adding a carry-adjusted returns table', '2026-03-28T10:00:00', '2026-04-02T14:00:00'),

('Options Skew as a Leading Indicator',
 'Research into predictive power of risk reversal skew for spot FX moves. Backtest across G10 pairs over 2020-2026 period. Preliminary results show 60% directional accuracy at 1-month horizon.',
 'Sarah Mitchell', 'Options & Volatility', 'options,risk-reversal,skew,backtest', 'InProgress', 3, 'Latest backtest figures look good. Add confidence intervals', '2026-03-15T12:00:00', '2026-04-04T16:00:00'),

('CAD and the Energy Transition',
 'Long-term thematic piece on how Canada energy transition away from fossil fuels may reshape the CAD correlation framework. Includes scenario analysis for 2030 CAD fair value.',
 'Emily Carter', 'Thematic Research', 'CAD,energy-transition,long-term,scenario', 'Draft', 1, '', '2026-03-20T14:00:00', NULL),

('AUD/NZD Relative Value Deep Dive',
 'Expanded version of the trade idea published April 2. Includes detailed fundamental comparison, terms-of-trade analysis, and central bank policy divergence framework.',
 'Liam O''Brien', 'Trade Ideas', 'AUD/NZD,relative-value,deep-dive', 'UnderReview', 2, 'Strengthen the ToT section with latest commodity data', '2026-04-02T12:00:00', '2026-04-04T11:00:00'),

('CBDC Cross-Border Settlement Framework',
 'Technical analysis of how CBDC interoperability protocols may affect FX settlement and pricing. Reviews mBridge, Project Dunbar, and Atlantic Council CBDC tracker data.',
 'Liam O''Brien', 'Thematic Research', 'CBDC,settlement,cross-border,fintech', 'Draft', 1, '', '2026-03-18T15:00:00', NULL),

('Scandinavian FX Mid-Year Review',
 'Comprehensive review of NOK, SEK, and DKK performance against EUR and USD. Covers central bank policy, oil dynamics, and relative valuation. Draft for June publication.',
 'Erik Svensson', 'Regional Review', 'NOK,SEK,DKK,Scandinavia,mid-year', 'InProgress', 1, '', '2026-04-05T08:00:00', NULL);

-------------------------------------------------------
-- ResearchPatterns (12 records)
-------------------------------------------------------
INSERT INTO ResearchPatterns (CurrencyPair, PatternName, Timeframe, Direction, ConfidenceScore, Description, DetectedBy, DetectedAt, ExpiresAt, Status) VALUES
('EUR/USD', 'Head and Shoulders', 'Daily', 'Sell', 78.50, 'Neckline at 1.0780, measured target at 1.0580. Right shoulder forming near 1.0870.', 'Marco Rossi', '2026-04-03T10:00:00', '2026-04-20T23:59:00', 'Active'),
('GBP/JPY', 'Rectangle Breakout', 'Weekly', 'Buy', 72.00, 'Compressing in 188.00-192.50 range for 8 weeks. Momentum divergence suggests upside breakout.', 'Marco Rossi', '2026-04-04T07:45:00', '2026-04-25T23:59:00', 'Active'),
('USD/JPY', 'Ascending Channel', 'Daily', 'Buy', 82.30, 'Trending higher within 149.50-155.00 channel since February. Channel support near 151.80.', 'Kenji Tanaka', '2026-04-01T04:00:00', '2026-05-01T23:59:00', 'Active'),
('AUD/NZD', 'Double Bottom', 'H4', 'Buy', 68.50, 'Double bottom at 1.0750 with confirmation above 1.0870. Target 1.1050.', 'Liam O''Brien', '2026-04-02T11:00:00', '2026-04-15T23:59:00', 'Active'),
('NOK/SEK', 'Falling Wedge', '4H', 'Buy', 65.00, 'Falling wedge forming since January. Breakout above 0.9750 triggers reversal target 0.9850.', 'Erik Svensson', '2026-03-22T09:30:00', '2026-04-15T23:59:00', 'Active'),
('EUR/USD', 'Bearish Engulfing', 'Weekly', 'Sell', 74.20, 'Weekly bearish engulfing candle on 1.0900 rejection. Confirms downtrend resumption.', 'Sarah Mitchell', '2026-03-29T08:00:00', '2026-04-12T23:59:00', 'Active'),
('USD/CNH', 'Ascending Triangle', 'Daily', 'Buy', 70.80, 'Flat resistance at 7.2800 with rising lows from 7.1500. Breakout imminent.', 'Kenji Tanaka', '2026-03-28T05:00:00', '2026-04-20T23:59:00', 'Active'),
('GBP/USD', 'Descending Triangle', 'Daily', 'Sell', 76.10, 'Flat support at 1.2500 with descending highs from 1.2720. Break targets 1.2300.', 'Emily Carter', '2026-04-02T10:00:00', '2026-04-25T23:59:00', 'Active'),
('USD/MXN', 'Bull Flag', 'H4', 'Buy', 60.50, 'Consolidation after impulse move 16.80 to 17.15. Flag breakout above 17.12 targets 17.45.', 'Liam O''Brien', '2026-03-25T16:00:00', '2026-04-10T23:59:00', 'Expired'),
('AUD/USD', 'Triple Top', 'Daily', 'Sell', 71.30, 'Three failed attempts at 0.6600 resistance. Neckline support at 0.6480. Break targets 0.6360.', 'Aisha Patel', '2026-03-28T01:00:00', '2026-04-18T23:59:00', 'Active'),
('EUR/SEK', 'Rising Wedge', 'Daily', 'Sell', 66.90, 'Rising wedge from 11.05 to 11.35 losing momentum. Breakdown below 11.18 targets 11.05.', 'Erik Svensson', '2026-04-01T08:00:00', '2026-04-20T23:59:00', 'Active'),
('USD/ZAR', 'Cup and Handle', 'Weekly', 'Buy', 63.40, 'Multi-month cup formation with handle near 18.20. Breakout above 18.55 targets 19.20.', 'Hugo Santos', '2026-04-04T06:30:00', '2026-05-31T23:59:00', 'Active');
