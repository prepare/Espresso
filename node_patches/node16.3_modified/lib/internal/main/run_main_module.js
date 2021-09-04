'use strict';

const {
  prepareMainThreadExecution
} = require('internal/bootstrap/pre_execution');

prepareMainThreadExecution(true);

markBootstrapComplete();

// Note: this loads the module through the ESM loader if the module is
// determined to be an ES module. This hangs from the CJS module loader
// because we currently allow monkey-patching of the module loaders
// in the preloaded scripts through require('module').
// runMain here might be monkey-patched by users in --require.
// XXX: the monkey-patchability here should probably be deprecated.
 
//////////////////////////////////
//#espresso, #6 
/////////////////////////////////
if (typeof LibEspresso !== 'undefined') {
    //we use our lib espr LibEspresso
    require('internal/modules/cjs/loader').Module.runMain("hello.espr");
    return;
} 
/////////////////////////////////
require('internal/modules/cjs/loader').Module.runMain(process.argv[1]);

