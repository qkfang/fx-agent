-- Seed mock data for ResearchArticles, Customers, and CustomerPortfolios
-- Run against the FxDatabase after migrations have been applied

-- Clear existing data (order matters for FK constraints)
DELETE FROM TraderNewsFeeds;
DELETE FROM TraderRecommendations;
DELETE FROM CustomerHistories;
DELETE FROM CustomerPreferences;
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
DBCC CHECKIDENT ('CustomerPreferences', RESEED, 0);
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
-- CustomerPreferences (10 records)
-------------------------------------------------------
INSERT INTO CustomerPreferences (CustomerId, PreferredCurrencyPairs, RiskTolerance, MaxPositionSize, StopLossPercent, TakeProfitPercent, TradingStyle, EnableNotifications, NotificationChannels, UpdatedAt) VALUES
(0, 'EUR/USD,GBP/USD',     'High',   200000.0000, 2.00, 4.00, 'Swing',    1, 'Email,SMS',   '2026-03-10T08:00:00'),
(1, 'USD/JPY,AUD/USD',     'Medium', 100000.0000, 1.50, 3.00, 'Day',      1, 'Email',       '2026-03-12T10:30:00'),
(2, 'EUR/GBP,USD/CHF',     'Low',    150000.0000, 1.00, 2.00, 'Position', 1, 'Email',       '2026-03-15T09:00:00'),
(3, 'USD/CNH,AUD/NZD',     'High',   300000.0000, 2.50, 5.00, 'Swing',    0, 'Email',       '2026-03-18T04:00:00'),
(4, 'EUR/SEK,NOK/SEK',     'Medium', 250000.0000, 1.50, 3.50, 'Day',      1, 'Email,Push',  '2026-03-20T08:45:00'),
(5, 'USD/AED,EUR/USD',     'Low',    500000.0000, 0.50, 1.50, 'Position', 1, 'Email,SMS',   '2026-03-22T10:00:00'),
(6, 'USD/KRW,GBP/JPY',     'High',   150000.0000, 2.00, 4.50, 'Scalp',    1, 'Push',        '2026-03-25T02:30:00'),
(7, 'USD/MXN,EUR/USD',     'Medium', 175000.0000, 1.50, 3.00, 'Swing',    0, 'Email',       '2026-03-28T16:00:00'),
(8, 'AUD/USD,NZD/USD',     'Medium', 60000.0000,  1.00, 2.50, 'Day',      1, 'Email,SMS',   '2026-04-01T01:00:00'),
(9, 'USD/ZAR,GBP/USD',     'High',   200000.0000, 3.00, 6.00, 'Position', 1, 'Email,Push',  '2026-04-02T12:00:00');

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
 'Forecast', 'Emily Carter', '2026-04-05T06:00:00', 'Draft', 'forecast,EUR/USD,GBP/USD,USD/JPY,outlook', 'Bearish'),

('EUR/USD Bulls Target 1.1000 as ECB Signals Pause',
 'The euro has surged to multi-month highs against the dollar after ECB officials signalled a prolonged pause in their rate-cut cycle. Technical breakout above 1.0900 opens the path to 1.1000.',
 '<p>The EUR/USD pair extended its rally for a third consecutive session on Friday, climbing to 1.0965 as European Central Bank policymakers reinforced their data-dependent stance and pushed back against aggressive easing expectations.</p><p><strong>Key Drivers:</strong></p><ul><li>ECB Governing Council member Schnabel warned that inflation stickiness in services could delay further cuts.</li><li>US non-farm payrolls missed estimates at +165k vs +200k expected, weakening dollar demand.</li><li>Euro-zone PMI data surprised to the upside, with the composite index returning to expansion at 50.4.</li></ul><p><strong>Technical Outlook:</strong></p><p>Having broken above the 200-day moving average at 1.0855, bulls are now targeting the psychological 1.1000 level. Immediate support sits at 1.0900, while a close below 1.0820 would invalidate the bullish bias. The RSI at 62 leaves room for further upside before overbought territory.</p><p><strong>Trading Recommendation:</strong> Long EUR/USD on dips towards 1.0900 with a target of 1.1050 and stop loss below 1.0820.</p>',
 'EUR/USD', 'Elena Marchetti', '2026-03-12T08:00:00', 'Published', 'EUR/USD,ECB,Technical Analysis,Bullish', 'Bullish'),

('GBP/USD Retreats as UK Inflation Undershoots Forecasts',
 'Sterling pulled back sharply from 1.2850 after UK CPI printed below consensus, raising expectations of an early Bank of England rate cut. Downside risk towards 1.2600 increases.',
 '<p>The British pound fell over 0.8% against the US dollar on Wednesday after official data showed UK consumer price inflation eased more than anticipated to 2.8% year-on-year in February, down from 3.0% in January and below the 2.9% market consensus.</p><p><strong>Key Drivers:</strong></p><ul><li>UK CPI fell to 2.8% YoY, core CPI dropped to 3.5% from 3.7%.</li><li>Money markets now price a 70% probability of a May BoE rate cut, up from 45% pre-data.</li><li>Dollar remained well-bid after Fed Chair Powell reiterated patience on rate cuts.</li></ul><p><strong>Technical Outlook:</strong></p><p>GBP/USD broke below the short-term uptrend support at 1.2800. Next key support lies at the 50-day moving average near 1.2700. A sustained break below 1.2700 would open the way towards 1.2600. Resistance is now seen at 1.2850.</p><p><strong>Trading Recommendation:</strong> Short GBP/USD on recoveries towards 1.2800 targeting 1.2650, stop above 1.2870.</p>',
 'GBP/USD', 'James Whitmore', '2026-03-11T10:30:00', 'Published', 'GBP/USD,Bank of England,CPI,Bearish', 'Bearish'),

('USD/JPY Consolidates Near 150.00 Ahead of BoJ Decision',
 'The yen is treading water near the critical 150.00 level as traders await the Bank of Japan''s policy announcement. A hawkish surprise could trigger sharp yen appreciation.',
 '<p>USD/JPY is consolidating in a narrow 149.50-150.50 range as market participants cautiously position ahead of the Bank of Japan''s policy meeting scheduled for next week. The pair has hovered around the psychologically important 150.00 level for the past two weeks.</p><p><strong>Key Drivers:</strong></p><ul><li>BoJ Governor Ueda hinted at further normalisation but stopped short of signalling an imminent hike.</li><li>Japan''s wage growth data for February showed a 3.2% increase, supporting the hawkish narrative.</li><li>US Treasury yields firmed, providing underlying support for USD/JPY.</li></ul><p><strong>Technical Outlook:</strong></p><p>The pair is forming a symmetrical triangle pattern. A break above 150.80 could trigger a move to 152.00, while a break below 149.20 would open the path to 148.00. Watch the BoJ press conference closely for directional cues.</p><p><strong>Trading Recommendation:</strong> Neutral - wait for BoJ resolution before initiating new positions. Favour a hawkish surprise trade: long JPY via short USD/JPY below 149.00 targeting 147.00.</p>',
 'USD/JPY', 'Kenji Tanaka', '2026-03-10T06:00:00', 'Published', 'USD/JPY,Bank of Japan,Yen,Neutral', 'Neutral'),

('AUD/USD: China Stimulus Hopes Lift Australian Dollar',
 'The Aussie rallied to 0.6580 as Beijing announced a broad fiscal stimulus package, boosting risk appetite and commodity demand. Further upside eyed toward 0.6650.',
 '<p>AUD/USD advanced 0.6% to 0.6580 on Thursday, its highest level since January, after China''s State Council unveiled a CNY 2 trillion fiscal stimulus package targeting infrastructure and domestic consumption. As China is Australia''s largest trading partner, the Aussie dollar is highly sensitive to Chinese economic developments.</p><p><strong>Key Drivers:</strong></p><ul><li>China''s CNY 2 trillion stimulus package boosted risk sentiment globally.</li><li>Iron ore prices rose 3.5% to $115/tonne, supporting the Australian dollar.</li><li>RBA minutes struck a balanced tone, not pre-committing to further cuts.</li><li>Global equity markets rallied 1.5-2%, boosting high-beta currencies including AUD.</li></ul><p><strong>Technical Outlook:</strong></p><p>The break above the 0.6550 resistance zone is constructive. The next resistance is at 0.6620 (November 2025 high), followed by 0.6650. Support is at 0.6520. The 14-day RSI has crossed above 55, signalling strengthening momentum.</p><p><strong>Trading Recommendation:</strong> Long AUD/USD on pullbacks to 0.6540 targeting 0.6650, stop below 0.6490.</p>',
 'AUD/USD', 'Sarah Lin', '2026-03-09T07:30:00', 'Published', 'AUD/USD,China,Stimulus,Bullish', 'Bullish'),

('Technical Analysis: Dollar Index Approaching Critical Resistance',
 'The US Dollar Index (DXY) is testing major resistance at 105.00. A decisive rejection could herald a broader dollar downtrend, reshaping FX landscape heading into Q2 2026.',
 '<p>The US Dollar Index (DXY) is at a crossroads. After a 3% rally from early-February lows, the index is testing the 105.00 level, a zone that served as both support and resistance multiple times over the past 18 months.</p><p><strong>Technical Picture:</strong></p><ul><li><strong>Resistance:</strong> 105.00 (key level), 105.60 (200-week MA)</li><li><strong>Support:</strong> 103.80 (50-day MA), 103.00 (trend support)</li><li><strong>RSI (14):</strong> 58 - approaching but not yet overbought</li><li><strong>MACD:</strong> Positive crossover in place but momentum slowing</li></ul><p><strong>Scenario Analysis:</strong></p><p><em>Bullish Dollar Scenario (probability 35%):</em> A clean break above 105.00 confirmed by a daily close opens the path to 106.50.</p><p><em>Bearish Dollar Scenario (probability 65%):</em> Rejection at 105.00 and reversal below 104.50 would signal the end of the corrective rally. Target 102.50 over 4-6 weeks.</p><p><strong>Key Events to Watch:</strong> US CPI (Tuesday), FOMC minutes (Wednesday), Fed Chair Powell speech (Friday).</p>',
 'Technical Analysis', 'Marcus Reeves', '2026-03-08T09:00:00', 'Published', 'DXY,USD,Technical Analysis,Resistance', 'Neutral'),

('Market Outlook: Fed Pivot Timeline Shifts - What It Means for FX',
 'Stronger US data has pushed the first Fed rate cut expectation back to September 2026. We examine how this shift reshapes the FX landscape across major pairs and EM currencies.',
 '<p>The repricing of Fed expectations following a string of upside US economic surprises has materially shifted the first cut expectation from June to September 2026. This has significant implications for global FX markets.</p><p><strong>Impact by Currency Pair:</strong></p><ul><li><strong>EUR/USD:</strong> The rate differential compression trade is delayed. EUR/USD upside capped near 1.1000 near-term.</li><li><strong>GBP/USD:</strong> BoE likely to cut before Fed now. Sterling faces additional headwinds; 1.2500 downside risk in H1 2026.</li><li><strong>USD/JPY:</strong> Elevated US rates keep upside pressure on pair, but BoJ normalisation should cap gains at 153.00.</li><li><strong>AUD/USD:</strong> Chinese stimulus provides an offsetting positive. AUD resilient vs USD despite dollar strength.</li><li><strong>EM Currencies:</strong> Delayed Fed cuts are negative for EM; expect continued pressure on BRL, TRY, and ZAR.</li></ul><p><strong>Macro Calendar Highlights:</strong> US CPI (March 18), ECB meeting (March 20), Fed meeting (March 22), BoJ decision (March 19).</p>',
 'Market Outlook', 'Dr. Alexandra Ross', '2026-03-07T11:00:00', 'Published', 'Fed,Rate Cuts,Market Outlook,FX Strategy', 'Neutral'),

('USD/CHF: Safe-Haven Demand Supports Franc Amid Geopolitical Tensions',
 'Renewed geopolitical uncertainty in Eastern Europe has driven safe-haven flows into the Swiss franc. USD/CHF dipped below parity - is it heading toward 0.9800?',
 '<p>The Swiss franc strengthened across the board on Monday as geopolitical tensions flared in Eastern Europe, driving traditional safe-haven demand into CHF and JPY. USD/CHF fell below the psychologically important parity level (1.0000) to trade at 0.9930.</p><p><strong>Key Drivers:</strong></p><ul><li>Geopolitical risk premium returned after ceasefire negotiations stalled.</li><li>SNB''s December statement reiterated willingness to intervene in FX market if needed - but current levels may not yet trigger action.</li><li>EUR/CHF fell to 1.0750, approaching SNB''s estimated comfort floor near 1.0650.</li></ul><p><strong>Technical Outlook:</strong></p><p>USD/CHF has broken below the ascending trend channel originating from October lows. The next support is at 0.9880 (2025 low), followed by 0.9800. A recovery above 1.0000 is needed to neutralise the bearish signal.</p><p><strong>Trading Recommendation:</strong> Cautious short USD/CHF below 0.9980 targeting 0.9880. Reduce exposure ahead of key geopolitical updates. Stop loss at 1.0050.</p>',
 'USD/CHF', 'Hans Weber', '2026-03-06T08:30:00', 'Published', 'USD/CHF,Swiss Franc,Safe Haven,Geopolitical', 'Bearish'),

('Fundamental Analysis: Why EUR/USD Could Break Above 1.1200 in Q2 2026',
 'A confluence of fundamental tailwinds - ECB pause, US fiscal concerns, and euro-zone recovery - presents a structural case for EUR/USD to reclaim levels not seen since early 2024.',
 '<p>Looking beyond near-term noise, a structural case is building for a sustained EUR/USD rally toward 1.1200 by the end of Q2 2026. This note outlines the three fundamental pillars supporting this view.</p><p><strong>1. ECB Rate Pause Creates Relative Yield Support</strong></p><p>With the ECB signalling a pause after four 25bp cuts, the EUR-USD 2-year swap spread has narrowed significantly.</p><p><strong>2. US Fiscal Concerns Weigh on Long-Term Dollar Outlook</strong></p><p>The US Congressional Budget Office projects the federal deficit to remain above 5% of GDP through 2028.</p><p><strong>3. Euro-zone Cyclical Recovery Underway</strong></p><p>After three quarters of stagnation, euro-zone GDP grew 0.4% in Q4 2025. Leading indicators suggest this recovery is broadening.</p><p><strong>12-Month Target: EUR/USD 1.1200.</strong> Build long positions on dips below 1.0900.</p>',
 'Fundamental Analysis', 'Elena Marchetti', '2026-03-05T07:00:00', 'Published', 'EUR/USD,ECB,Fundamental Analysis,Q2 Outlook', 'Bullish'),

('GBP/JPY Weekly Technical Note',
 'Preliminary technical analysis of GBP/JPY ahead of the upcoming BoJ and BoE decisions. This note is pending final review before publication.',
 '<p>This is a draft research note in progress...</p>',
 'Technical Analysis', 'James Whitmore', '2026-03-13T12:00:00', 'Draft', 'GBP/JPY,Technical Analysis', 'Neutral');

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
