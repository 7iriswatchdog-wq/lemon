/**
 * PDF Export Utility for AML Web Application
 * Uses html2pdf.js (self-hosted in ~/lib/html2pdf/html2pdf.bundle.min.js)
 */

async function downloadPageAsPDF(containerSelector, filename = 'ProcessDetails.pdf', orientation = 'portrait') {
    const original = document.querySelector(containerSelector);
    if (!original) {
        console.error('Container not found:', containerSelector);
        return;
    }

    // Show loader if available (checking both current and parent context)
    const getLoader = () => {
        const ids = ['loadingeffect', 'processLoader', 'loader', 'loadingCaseDetails', 'loadingOverlay'];
        for (const id of ids) {
            const el = document.getElementById(id) || (window.parent ? window.parent.document.getElementById(id) : null);
            if (el) return el;
        }
        return null;
    };

    const loader = getLoader();
    if (loader) {
        loader.style.setProperty('display', 'flex', 'important');
        loader.classList.remove('hidden');
    }

    try {
        // --- STEP 1: PRE-CAPTURE DATA FROM ORIGINAL ---
        const originalInputs = original.querySelectorAll('select, textarea, input:not([type="hidden"])');
        const capturedData = Array.from(originalInputs).map(el => {
            let val = '';
            if (el.tagName === 'SELECT') {
                val = el.options[el.selectedIndex]?.text || '';
            } else if (el.tagName === 'TEXTAREA' || el.tagName === 'INPUT') {
                val = el.value || '';
            }
            return {
                type: el.tagName,
                value: val,
                checked: (el.type === 'checkbox' || el.type === 'radio') ? el.checked : null
            };
        });

        // --- STEP 2: CLONE AND PREPARE ---
        const clone = original.cloneNode(true);

        // Apply captured data to clone
        const cloneInputs = clone.querySelectorAll('select, textarea, input:not([type="hidden"])');
        cloneInputs.forEach((el, idx) => {
            const data = capturedData[idx];
            if (data) {
                el.setAttribute('data-pdf-val', data.value);
                if (data.checked !== null) el.setAttribute('data-pdf-checked', data.checked);
            }
        });

        // Create a temporary container for styling and dimensioning
        const tempContainer = document.createElement('div');
        tempContainer.id = 'pdf-render-temp';
        tempContainer.style.cssText = `
            position: absolute;
            left: -9999px;
            top: 0;
            width: ${orientation === 'landscape' ? '1280px' : '1024px'}; 
            max-width: ${orientation === 'landscape' ? '1280px' : '1024px'}; 
            min-width: ${orientation === 'landscape' ? '1280px' : '1024px'}; 
            margin: 0 auto;
        `;
        tempContainer.appendChild(clone);
        document.body.appendChild(tempContainer);

        // --- STEP 3: TRANSFORM CLONE FOR PDF ---
        clone.querySelectorAll('.flex-col, .flex-row, .flex, .grid').forEach(el => {
            if (el.classList.contains('rounded-lg') || el.classList.contains('card') || el.classList.contains('rounded-xl')) {
                el.style.display = 'block';
                el.style.overflow = 'visible';
            }
        });

        const uiArtifacts = clone.querySelectorAll('.dt-loader, .spinner-grow, .loader, .loadingeffect, .slider-pagination, .no-pdf, button, .pagination, .loading-dots');
        uiArtifacts.forEach(el => el.remove());

        clone.querySelectorAll('[data-pdf-val]').forEach(el => {
            const val = el.getAttribute('data-pdf-val');
            const replacement = document.createElement('span');
            replacement.innerText = val || '-';
            if (el.className) replacement.className = el.className;
            el.parentNode.replaceChild(replacement, el);
        });

        clone.querySelectorAll('table').forEach(table => {
            table.style.width = '100%';
        });

        clone.querySelectorAll('.overflow-y-auto, .custom-scrollbar, .overflow-hidden').forEach(el => {
            el.style.overflow = 'visible';
            el.style.maxHeight = 'none';
            el.style.height = 'auto';
        });

        clone.querySelectorAll('tr, .info-box, .pl-6.relative, .timeline-item').forEach(el => {
            el.style.pageBreakInside = 'avoid';
            el.style.breakInside = 'avoid';
        });

        // --- STEP 4: GENERATE PDF ---
        const opt = {
            margin: [5, 10, 10, 10],
            filename: filename,
            image: { type: 'jpeg', quality: 0.98 },
            html2canvas: {
                scale: 2,
                useCORS: true,
                letterRendering: true,
                backgroundColor: '#ffffff',
                logging: false,
                scrollY: 0,
                scrollX: 0
            },
            jsPDF: { unit: 'mm', format: 'a4', orientation: orientation },
            pagebreak: { mode: ['css', 'legacy'] }
        };

        await new Promise(resolve => setTimeout(resolve, 500));
        window.scrollTo(0, 0);
        await html2pdf().set(opt).from(clone).save();

        document.body.removeChild(tempContainer);

    } catch (error) {
        console.error('PDF Generation failed:', error);
        if (typeof toastr !== 'undefined') toastr.error('Failed to generate PDF');
    } finally {
        if (loader) {
            loader.style.display = 'none';
            loader.classList.add('hidden');
        }
    }
}
