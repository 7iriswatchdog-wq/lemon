/**
 * PDF Export Utility for AML Web Application
 * Uses html2pdf.js (requires https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js)
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
            width: 1024px; 
            background: white;
            padding: 20px;
        `;
        tempContainer.appendChild(clone);
        document.body.appendChild(tempContainer);

        // --- STEP 3: TRANSFORM CLONE FOR PDF ---

        // A. Layout Stacking (Two-column to Single-column)
        const leftCol = clone.querySelector('#leftColumn');
        const rightCol = clone.querySelector('#rightColumn');
        if (leftCol && rightCol) {
            const container = leftCol.parentElement;
            container.style.display = 'block'; 
            leftCol.style.width = '100%';
            leftCol.style.marginBottom = '20px';
            rightCol.style.width = '100%';
            
            // Remove constraints on containers
            const constrained = rightCol.querySelectorAll('[class*="max-h-"], [class*="min-h-"], [style*="height"]');
            constrained.forEach(el => {
                el.style.maxHeight = 'none';
                el.style.minHeight = '0';
                el.style.height = 'auto';
                el.style.overflow = 'visible';
            });
        }

        // B. Clear All Loaders, Spinners, and Pagination artifacts
        const uiArtifacts = clone.querySelectorAll('.dt-loader, .spinner-grow, .loader, .loadingeffect, .slider-pagination, .no-pdf, button, .pagination, .loading-dots');
        uiArtifacts.forEach(el => el.remove());

        // C. Transform Inputs/Selects to clean Static Text
        clone.querySelectorAll('[data-pdf-val]').forEach(el => {
            const val = el.getAttribute('data-pdf-val');
            const replacement = document.createElement('div');
            
            if (el.tagName === 'SELECT') {
                replacement.className = 'inline-flex items-center px-4 py-1 rounded-full text-[10px] font-bold uppercase bg-blue-50 text-blue-700 ring-1 ring-inset ring-blue-100 ml-auto min-w-[80px] justify-center';
                replacement.innerText = (val === '--Select--' || !val) ? '--EMPTY--' : val;
            } else if (el.tagName === 'TEXTAREA') {
                replacement.className = 'text-[11px] font-medium text-slate-700 p-3 border border-slate-100 rounded bg-slate-50 mt-1 whitespace-pre-wrap w-full';
                replacement.innerText = val || '(No remarks provided)';
            } else {
                replacement.className = 'text-[11px] font-semibold text-slate-800';
                replacement.innerText = val || '-';
            }
            
            el.parentNode.replaceChild(replacement, el);
        });

        // D. Table Polish (Fixing "Trimmed Columns")
        clone.querySelectorAll('table').forEach(table => {
            table.style.width = '100%';
            table.style.tableLayout = 'auto'; // Change to auto to allow columns to fit content better
            table.style.borderCollapse = 'collapse';
            table.classList.remove('table-fixed');
            
            table.querySelectorAll('th, td').forEach(cell => {
                cell.style.padding = '8px 6px';
                cell.style.fontSize = '10px';
                cell.style.wordBreak = 'break-word';
                cell.style.borderBottom = '1px solid #f1f5f9';
                cell.style.textAlign = 'left';
            });

            // Specific fix for Search Results table headers
            table.querySelectorAll('thead th').forEach(th => {
                th.style.backgroundColor = '#f8fafc';
                th.style.color = '#64748b';
            });
        });

        // E. Expand all Overflow containers
        clone.querySelectorAll('.overflow-y-auto, .custom-scrollbar, .overflow-hidden').forEach(el => {
            el.style.overflow = 'visible';
            el.style.maxHeight = 'none';
            el.style.height = 'auto';
        });

        // F. Page Break Optimization
        clone.querySelectorAll('.card, .info-box, table, tr, .process-section').forEach(el => {
            el.style.pageBreakInside = 'avoid';
            el.style.breakInside = 'avoid';
        });

        // --- STEP 4: GENERATE PDF ---
        const opt = {
            margin:       [10, 5, 10, 5], 
            filename:     filename,
            image:        { type: 'jpeg', quality: 0.98 },
            html2canvas:  { 
                scale: 2, 
                useCORS: true, 
                letterRendering: true,
                backgroundColor: '#ffffff',
                logging: false
            },
            jsPDF:        { unit: 'mm', format: 'a4', orientation: 'portrait' },
            pagebreak:    { mode: ['avoid-all', 'css', 'legacy'] }
        };

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
