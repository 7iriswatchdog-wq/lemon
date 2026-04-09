/**
 * PDF Export Utility for AML Web Application
 * Uses html2pdf.js (self-hosted in ~/lib/html2pdf.js/html2pdf.bundle.min.js)
 */

async function downloadPageAsPDF(containerSelector, filename = 'ProcessDetails.pdf') {
    const original = document.querySelector(containerSelector);
    if (!original) {
        console.error('Container not found:', containerSelector);
        return;
    }

    // Show loader if available
    const loader = document.getElementById('loadingeffect') || document.getElementById('processLoader');
    if (loader) loader.style.display = 'flex';

    try {
        // --- STEP 1: PRE-CAPTURE DATA FROM ORIGINAL ---
        // cloneNode doesn't capture current dynamic values/states well in all browsers/frameworks.
        // We'll read the original elements and prepare data for the clone.
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
            width: 800px; 
            background: white;
            padding: 0;
        `;
        tempContainer.appendChild(clone);
        document.body.appendChild(tempContainer);

        // --- STEP 3: TRANSFORM CLONE FOR PDF ---

        // A. Layout Stability (Convert Flex to Block for PDF)
        clone.querySelectorAll('.flex-col, .flex-row, .flex, .grid').forEach(el => {
            // We only convert major containers that might cause "gaps" in PDF renderers
            if (el.classList.contains('rounded-lg') || el.classList.contains('card') || el.classList.contains('rounded-xl')) {
                el.style.display = 'block';
                el.style.overflow = 'visible';
            }
        });

        const leftCol = clone.querySelector('#leftColumn');
        const rightCol = clone.querySelector('#rightColumn');
        if (leftCol && rightCol) {
            // If they are side-by-side but causing overflow, we can stack, 
            // but for Process_PDF we expect them to be well-behaved.
            // leftCol.parentElement.style.display = 'block'; 
        }

        // B. Clear All Loaders, Spinners, and Pagination artifacts
        const uiArtifacts = clone.querySelectorAll('.dt-loader, .spinner-grow, .loader, .loadingeffect, .slider-pagination, .no-pdf, button, .pagination, .loading-dots');
        uiArtifacts.forEach(el => el.remove());

        // C. Clean Dynamic Inputs (keep local styles)
        clone.querySelectorAll('[data-pdf-val]').forEach(el => {
            const val = el.getAttribute('data-pdf-val');
            const replacement = document.createElement('span');
            replacement.innerText = val || '-';
            // Inherit parent classes for minimal disruption
            if (el.className) replacement.className = el.className;
            el.parentNode.replaceChild(replacement, el);
        });

        // D. Respect Existing Table Styles
        clone.querySelectorAll('table').forEach(table => {
            table.style.width = '100%';
            // We trust Process_PDF for tableLayout, borderCollapse, padding, and font sizes
        });

        // E. Expand all Overflow containers
        clone.querySelectorAll('.overflow-y-auto, .custom-scrollbar, .overflow-hidden').forEach(el => {
            el.style.overflow = 'visible';
            el.style.maxHeight = 'none';
            el.style.height = 'auto';
        });

        // F. Page Break Optimization
        clone.querySelectorAll('tr, .info-box, .pl-6.relative, .timeline-item').forEach(el => {
            el.style.pageBreakInside = 'avoid';
            el.style.breakInside = 'avoid';
        });

        // --- STEP 4: GENERATE PDF ---
        const opt = {
            margin:       [5, 12, 10, 12], 
            filename:     filename,
            image:        { type: 'jpeg', quality: 0.98 },
            html2canvas:  { 
                scale: 2, 
                useCORS: true, 
                letterRendering: true,
                backgroundColor: '#ffffff',
                logging: false,
                scrollY: 0,
                scrollX: 0
            },
            jsPDF:        { unit: 'mm', format: 'a4', orientation: 'portrait' },
            pagebreak:    { mode: ['css', 'legacy'] }
        };

        // G. Add a small delay for Lucide icons and Tailwind styles to settle in clone
        await new Promise(resolve => setTimeout(resolve, 500));

        window.scrollTo(0, 0); // Ensure window is at top
        await html2pdf().set(opt).from(clone).save();

        // Cleanup
        document.body.removeChild(tempContainer);

    } catch (error) {
        console.error('PDF Generation failed:', error);
        if (typeof toastr !== 'undefined') toastr.error('Failed to generate PDF');
    } finally {
        if (loader) loader.style.display = 'none';
    }
}
