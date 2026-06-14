// MongoDB Playground
// Use Ctrl+Space inside a snippet or a string literal to trigger completions.

// The current database to use.
use('AMLScreening');

// Create a new document in the collection.
const fs = require('fs');

const filesToInsert = [
    { name: 'BLACKLIST', path: 'o:/NC/lemon/BLACKLIST_lemon_04012026.json' },
    { name: 'CASELOG', path: 'o:/NC/lemon/CASELOG_lemon_04012026.json' },
    { name: 'NAMELIST', path: 'o:/NC/lemon/NAMELIST_lemon_04012026.json' },
    { name: 'PF_SEARCHRESULTS', path: 'o:/NC/lemon/PF_SEARCHRESULTS_lemon_04012026.json' }
];

for (const file of filesToInsert) {
    if (fs.existsSync(file.path)) {
        console.log(`Reading ${file.name} data...`);
        // Use EJSON.parse instead of JSON.parse to properly deserialize $oid, $date, etc.
        const data = EJSON.parse(fs.readFileSync(file.path, 'utf8'));
        console.log(`Inserting ${data.length} records into ${file.name}...`);
        db.getCollection(file.name).insertMany(data);
    } else {
        console.log(`File not found: ${file.path}`);
    }
}
