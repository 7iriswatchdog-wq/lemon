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
        // Clone the element
        const clone = original.cloneNode(true);
        
        // --- DATA SYNC (cloneNode doesn't copy current form state) ---
        const syncInputs = (origRoot, cloneRoot) => {
            const selects = origRoot.querySelectorAll('select');
            const cloneSelects = cloneRoot.querySelectorAll('select');
            selects.forEach((sel, i) => {
                cloneSelects[i].selectedIndex = sel.selectedIndex;
                cloneSelects[i].value = sel.value;
            });

            const textareas = origRoot.querySelectorAll('textarea');
            const cloneTextareas = cloneRoot.querySelectorAll('textarea');
            textareas.forEach((ta, i) => {
                cloneTextareas[i].value = ta.value;
            });

            const inputs = origRoot.querySelectorAll('input:not([type="hidden"])');
            const cloneInputs = cloneRoot.querySelectorAll('input:not([type="hidden"])');
            inputs.forEach((input, i) => {
                if (input.type === 'checkbox' || input.type === 'radio') {
                    cloneInputs[i].checked = input.checked;
                } else {
                    cloneInputs[i].value = input.value;
                }
            });
        };
        syncInputs(original, clone);

        // Create a temporary container for the clone
        const tempContainer = document.createElement('div');
        tempContainer.style.position = 'absolute';
        tempContainer.style.left = '-9999px';
        tempContainer.style.top = '0';
        tempContainer.style.width = '1100px'; // Controlled width for A4
        tempContainer.appendChild(clone);
        document.body.appendChild(tempContainer);

        // --- PDF LAYOUT ADJUSTMENTS ---

        // 1. Stack Two-Column Layouts
        const leftCol = clone.querySelector('#leftColumn');
        const rightCol = clone.querySelector('#rightColumn');
        const mainFlexContainer = leftCol?.parentElement;

        if (mainFlexContainer && leftCol && rightCol) {
            mainFlexContainer.classList.remove('flex-row', 'gap-6');
            mainFlexContainer.classList.add('flex-col', 'gap-4');
            
            leftCol.classList.remove('basis-[65%]');
            leftCol.classList.add('basis-full', 'w-full');
            
            rightCol.classList.remove('basis-[35%]');
            rightCol.classList.add('basis-full', 'w-full');
            
            // Remove fixed heights on risk containers
            const riskContainer = rightCol.querySelector('.min-h-\\[260px\\]');
            if (riskContainer) {
                riskContainer.classList.remove('min-h-[260px]', 'max-h-[260px]');
                riskContainer.style.minHeight = '0';
                riskContainer.style.maxHeight = 'none';
            }
        }

        // 2. Expand Scrollable Containers
        const scrollables = clone.querySelectorAll('.overflow-y-auto, .custom-scrollbar, [style*="max-height"], [class*="max-h-"], [class*="min-h-"]');
        scrollables.forEach(el => {
            el.classList.remove('overflow-y-auto', 'custom-scrollbar', 'overflow-hidden');
            // Remove all max-h-* and min-h-* classes
            const classesToRemove = Array.from(el.classList).filter(c => c.startsWith('max-h-') || c.startsWith('min-h-'));
            classesToRemove.forEach(c => el.classList.remove(c));
            
            el.style.maxHeight = 'none';
            el.style.minHeight = '0';
            el.style.height = 'auto';
            el.style.overflow = 'visible';
        });

        // 3. Hide non-PDF elements and transform inputs to text
        const noPdfElements = clone.querySelectorAll('.no-pdf, button, .dt-loader, .spinner-grow, #loadingeffect, #processLoader');
        noPdfElements.forEach(el => el.style.display = 'none');

        // Transform interactive elements to static text for clean PDF
        clone.querySelectorAll('textarea').forEach(el => {
            const textSpan = document.createElement('div');
            textSpan.className = 'text-[11px] font-medium text-slate-700 p-2 border border-slate-100 rounded bg-slate-50 mt-1 whitespace-pre-wrap';
            textSpan.innerText = el.value || '(No remarks)';
            el.parentNode.replaceChild(textSpan, el);
        });

        clone.querySelectorAll('select').forEach(el => {
            const textSpan = document.createElement('span');
            textSpan.className = 'inline-flex items-center px-4 py-1 rounded-full text-[10px] font-bold uppercase bg-slate-100 text-slate-800 ring-1 ring-inset ring-slate-200 ml-auto';
            textSpan.innerText = el.options[el.selectedIndex]?.text || '-';
            el.parentNode.replaceChild(textSpan, el);
        });

        // 4. Clean up tables
        clone.querySelectorAll('table').forEach(table => {
            table.style.width = '100%';
            table.style.tableLayout = 'auto';
            table.classList.remove('table-fixed');
            // Ensure table headers repeat if possible (html2pdf limitation but helps)
        });

        // 5. Hide the specific blue dots (loading artifacts)
        const loaders = clone.querySelectorAll('.dt-loader, .loader, .spinner-grow');
        loaders.forEach(l => l.style.display = 'none');

        // --- GENERATE PDF ---
        const opt = {
            margin:       10,
            filename:     filename,
            image:        { type: 'jpeg', quality: 0.98 },
            html2canvas:  { 
                scale: 1.5, // Reduced slightly for memory, increased for quality
                useCORS: true, 
                letterRendering: true,
                logging: false,
                backgroundColor: '#ffffff'
            },
            jsPDF:        { unit: 'mm', format: 'a4', orientation: 'portrait' }
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
