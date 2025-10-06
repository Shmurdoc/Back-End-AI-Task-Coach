const newman = require('newman');
const fs = require('fs');

let env = undefined;
const envPath = './postman/environment.postman_environment.json';
if (fs.existsSync(envPath)) {
  try {
    env = require(envPath);
    console.log('Loaded Postman environment:', envPath);
  } catch (e) {
    console.warn('Failed loading environment file:', e.message);
  }
}

newman.run({
  collection: require('./Doc/Postman_Collection.json'),
  reporters: 'cli',
  environment: env,
  timeoutRequest: 10000,
}, function (err, summary) {
  if (err || summary.run.failures && summary.run.failures.length) {
    console.error('Postman collection run found failures');
    process.exit(2);
  }
  console.log('Postman collection run complete!');
  process.exit(0);
});
