(function ($) {
    const statusClasses = {
        OnTime: 'bg-success-subtle text-success',
        Delayed: 'bg-warning-subtle text-dark',
        OffRoute: 'bg-danger-subtle text-danger'
    };

    const cardUpdateHandles = new Map();
    const updateIntervalMs = 5000;

    function updateTextIfChanged($el, value) {
        const textValue = value ?? '';
        if ($el.text() !== textValue) {
            $el.text(textValue);
            $el.addClass('pulse');
            setTimeout(() => $el.removeClass('pulse'), 600);
        }
    }

    function renderTimeline($container, stops) {
        const existing = $container.find('.timeline-stop');
        if (existing.length !== stops.length) {
            $container.empty();
            stops.forEach(stop => {
                $container.append(buildStop(stop));
            });
            return;
        }

        existing.each(function (index) {
            const stop = stops[index];
            const $stop = $(this);
            $stop.toggleClass('complete', !!stop.isComplete);
            updateTextIfChanged($stop.find('.fw-semibold'), stop.name);
            updateTextIfChanged($stop.find('.text-muted'), stop.arrivesAt);
        });
    }

    function buildStop(stop) {
        return $(
            `<div class="timeline-stop ${stop.isComplete ? 'complete' : ''}">` +
            '  <div class="dot"></div>' +
            '  <div>' +
            `    <div class="fw-semibold">${stop.name}</div>` +
            `    <div class="small text-muted">${stop.arrivesAt}</div>` +
            '  </div>' +
            '</div>'
        );
    }

    function renderProbills($container, probills) {
        const map = new Map();
        $container.find('.probill-chip').each(function () {
            map.set($(this).data('probill'), $(this));
        });

        probills.forEach(pb => {
            if (!map.has(pb.number)) {
                $container.append(buildProbill(pb));
            }
            const $chip = map.get(pb.number) ?? $container.find(`[data-probill="${pb.number}"]`);
            updateTextIfChanged($chip.find('.small.text-muted'), `${pb.pickupLocation} → ${pb.deliveryLocation}`);
            updateTextIfChanged($chip.find('.arrival'), pb.stopArrival ?? 'Pending');
        });
    }

    function buildProbill(pb) {
        return $(
            `<div class="probill-chip" data-probill="${pb.number}">` +
            `  <div class="fw-semibold">${pb.number}</div>` +
            `  <div class="small text-muted">${pb.pickupLocation} → ${pb.deliveryLocation}</div>` +
            `  <div class="small arrival">${pb.stopArrival ?? 'Pending'}</div>` +
            '</div>'
        );
    }

    function renderMeta($section, trip) {
        updateTextIfChanged($section.find('[data-field="truck"]'), trip.truckUnit);
        updateTextIfChanged($section.find('[data-field="driver"]'), trip.driverName);
        updateTextIfChanged($section.find('[data-field="trailers"]'), trip.trailers.map(t => t.unitNumber).join(', '));
        updateTextIfChanged($section.find('[data-field="customers"]'), trip.customers.map(c => c.name).join(' • '));
    }

    function updateStatusBadge($badge, status) {
        const cls = statusClasses[status] ?? 'bg-secondary-subtle';
        if (!$badge.hasClass(cls)) {
            $badge.removeClass(Object.values(statusClasses).join(' ')).addClass(cls);
        }
        updateTextIfChanged($badge, status);
    }

    function buildCard(trip) {
        return $(
            `<div class="card trip-card shadow-sm" data-trip-id="${trip.id}">` +
            '  <div class="card-header bg-white border-0 d-flex justify-content-between align-items-start">' +
            '    <div>' +
            '      <div class="text-muted small">Trip window</div>' +
            `      <div class="fw-semibold">${trip.startDate} - ${trip.endDate}</div>` +
            '    </div>' +
            `    <span class="badge status-badge ${statusClasses[trip.status]}" data-section="status">${trip.status}</span>` +
            '  </div>' +
            '  <div class="card-body pt-0">' +
            '    <div class="row g-4">' +
            '      <div class="col-12 col-lg-4">' +
            '        <div class="trip-meta" data-section="meta">' +
            '          <div class="d-flex align-items-center mb-2">' +
            '            <div class="circle-icon bg-primary-subtle text-primary me-2">🚚</div>' +
            '            <div>' +
            '              <div class="text-muted small">Truck</div>' +
            `              <div class="fw-semibold" data-field="truck">${trip.truckUnit}</div>` +
            '            </div>' +
            '          </div>' +
            '          <div class="d-flex align-items-center mb-2">' +
            '            <div class="circle-icon bg-info-subtle text-info me-2">👤</div>' +
            '            <div>' +
            '              <div class="text-muted small">Driver</div>' +
            `              <div class="fw-semibold" data-field="driver">${trip.driverName}</div>` +
            '            </div>' +
            '          </div>' +
            '          <div class="d-flex align-items-center mb-2">' +
            '            <div class="circle-icon bg-secondary-subtle text-secondary me-2">🛞</div>' +
            '            <div>' +
            '              <div class="text-muted small">Trailers</div>' +
            `              <div class="fw-semibold" data-field="trailers">${trip.trailers.map(t => t.unitNumber).join(', ')}</div>` +
            '            </div>' +
            '          </div>' +
            '          <div class="d-flex align-items-center">' +
            '            <div class="circle-icon bg-warning-subtle text-warning me-2">🏢</div>' +
            '            <div>' +
            '              <div class="text-muted small">Customers</div>' +
            `              <div class="fw-semibold" data-field="customers">${trip.customers.map(c => c.name).join(' • ')}</div>` +
            '            </div>' +
            '          </div>' +
            '        </div>' +
            '      </div>' +
            '      <div class="col-12 col-lg-5">' +
            '        <div class="probill" data-section="probills">' +
            '          <div class="text-muted small mb-2">Probills</div>' +
            '          <div class="probill-list"></div>' +
            '        </div>' +
            '      </div>' +
            '      <div class="col-12 col-lg-3">' +
            '        <div class="text-muted small mb-2">Timeline</div>' +
            '        <div class="timeline" data-section="timeline"></div>' +
            '      </div>' +
            '    </div>' +
            '  </div>' +
            '</div>'
        );
    }

    function renderCardContent($card, trip) {
        updateStatusBadge($card.find('[data-section="status"]'), trip.status);
        renderMeta($card.find('[data-section="meta"]'), trip);
        renderProbills($card.find('.probill-list'), trip.probills);
        renderTimeline($card.find('[data-section="timeline"]'), trip.timeline);
    }

    function rebuildCards(trips) {
        const $grid = $('#trip-grid');
        $grid.empty();
        const fragment = $(document.createDocumentFragment());
        trips.forEach(trip => {
            const $card = buildCard(trip);
            renderProbills($card.find('.probill-list'), trip.probills);
            renderTimeline($card.find('[data-section="timeline"]'), trip.timeline);
            fragment.append($card);
        });
        $grid.append(fragment);
        attachHoverPause();
    }

    function attachHoverPause() {
        $('.trip-card').each(function () {
            const id = $(this).data('trip-id');
            $(this).on('mouseenter', () => cardUpdateHandles.set(id, true));
            $(this).on('mouseleave', () => cardUpdateHandles.delete(id));
        });
    }

    function fetchTrips() {
        const order = $('#orderSelect').val();
        const critical = $('#criticalSwitch').is(':checked') ? 'critical' : 'all';
        return $.getJSON('/api/trips', { order, critical });
    }

    function applyUpdate(trips) {
        const cards = new Map();
        $('.trip-card').each(function () {
            cards.set($(this).data('trip-id'), $(this));
        });

        let structureChanged = trips.length !== cards.size;
        if (!structureChanged) {
            for (const trip of trips) {
                if (!cards.has(trip.id)) {
                    structureChanged = true;
                    break;
                }
            }
        }

        if (structureChanged) {
            rebuildCards(trips);
            return;
        }

        trips.forEach(trip => {
            const $card = cards.get(trip.id);
            if (!$card || cardUpdateHandles.has(trip.id)) {
                return;
            }
            renderCardContent($card, trip);
        });
    }

    function scheduleUpdates() {
        setInterval(() => {
            fetchTrips().done(applyUpdate);
        }, updateIntervalMs);
    }

    function hydrateFromServer() {
        const payload = $('#trip-grid').data('initial-trips');
        const trips = Array.isArray(payload) ? payload : [];
        rebuildCards(trips);
    }

    $(function () {
        hydrateFromServer();
        scheduleUpdates();

        $('#orderSelect, #criticalSwitch').on('change', () => {
            fetchTrips().done(applyUpdate);
        });
    });
}(jQuery));
