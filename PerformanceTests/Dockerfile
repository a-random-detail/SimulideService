# Use official k6 image
FROM grafana/k6:latest

# Copy the k6 test script into the container
COPY performance-test.js /performance-test.js

# Command to run the test
ENTRYPOINT ["k6", "run", "/performance-test.js"]