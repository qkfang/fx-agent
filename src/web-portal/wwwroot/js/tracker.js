(function () {
    // ── Session ID ────────────────────────────────────────────────────────────
    var sessionId = localStorage.getItem('fxr_session');
    if (!sessionId) {
        sessionId = (typeof crypto !== 'undefined' && crypto.randomUUID)
            ? crypto.randomUUID()
            : 'sess_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
        localStorage.setItem('fxr_session', sessionId);
    }

    // ── Entry time & click counter ────────────────────────────────────────────
    var entryTime = Date.now();
    var clickCount = 0;
    document.addEventListener('click', function () { clickCount++; });

    // ── Article ID from meta tag (set on Article page) ────────────────────────
    var articleIdMeta = document.querySelector('meta[name="article-id"]');
    var articleId = articleIdMeta ? parseInt(articleIdMeta.content, 10) : null;

    // ── Browser info ──────────────────────────────────────────────────────────
    var language = navigator.language || '';
    var timezone = '';
    try { timezone = Intl.DateTimeFormat().resolvedOptions().timeZone || ''; } catch (e) { }
    var screenSize = screen.width + 'x' + screen.height;

    // ── Build payload ─────────────────────────────────────────────────────────
    function buildPayload() {
        var nameEl = document.getElementById('lead-name');
        var emailEl = document.getElementById('lead-email');
        var companyEl = document.getElementById('lead-company');
        return {
            sessionId: sessionId,
            articleId: articleId,
            pageUrl: window.location.pathname + window.location.search,
            timeSpentSeconds: Math.round((Date.now() - entryTime) / 1000),
            clickCount: clickCount,
            language: language,
            timezone: timezone,
            screenSize: screenSize,
            referrer: document.referrer || '',
            userName: nameEl ? nameEl.value : '',
            userEmail: emailEl ? emailEl.value : '',
            userCompany: companyEl ? companyEl.value : ''
        };
    }

    // ── Send via sendBeacon (fire-and-forget on page unload) ──────────────────
    function sendBeacon() {
        var payload = buildPayload();
        var blob = new Blob([JSON.stringify(payload)], { type: 'application/json' });
        navigator.sendBeacon('/api/track', blob);
    }

    window.addEventListener('beforeunload', sendBeacon);
    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'hidden') sendBeacon();
    });

    // ── Lead capture form (inline submit, no navigation) ─────────────────────
    var leadForm = document.getElementById('lead-form');
    if (leadForm) {
        leadForm.addEventListener('submit', function (e) {
            e.preventDefault();
            var payload = buildPayload();
            fetch('/api/track', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            }).then(function () {
                var div = document.createElement('div');
                div.className = 'lead-success';
                div.textContent = '✓ Thank you! We\'ll be in touch with exclusive research.';
                leadForm.replaceWith(div);
            }).catch(function () {
                var div = document.createElement('div');
                div.className = 'alert alert-danger';
                div.textContent = 'Something went wrong. Please try again.';
                leadForm.replaceWith(div);
            });
        });
    }
})();
