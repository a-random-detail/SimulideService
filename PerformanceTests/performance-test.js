import http from 'k6/http';
import ws from 'k6/ws';
import { check, sleep } from 'k6';

export let options = {
    stages: [
      { duration: '1m', target: 500 },
      { duration: '3m', target: 1000 },
      { duration: '1m', target: 0 },
    ],
    ext: {
      loadimpact: {
        projectID: 111111, // Optional for LoadImpact cloud testing
        name: "k6 performance test",
      },
    },
  };
function createDocument() {
  let res = http.post('http://localhost:8080/documents', JSON.stringify({
    name: 'Test Document',
    content: 'Hello World',
    version: 1,
  }), { headers: { 'Content-Type': 'application/json' } });

  check(res, { 'Document created successfully': (r) => r.status === 201 });

  let responseBody = JSON.parse(res.body);
  return responseBody.data.id; // Extract document ID
}

function fetchDocument(documentId) {
  let res = http.get(`http://localhost:8080/documents/${documentId}`);

  check(res, {
    'Fetched document successfully': (r) => r.status === 200,
  });

  sleep(1);
}

function testSignalR(documentId) {
  let url = 'ws://localhost/collaboration';

  let res = ws.connect(url, function (socket) {
    socket.on('open', function () {
      socket.send('{"protocol":"json","version":1}\x1e') //
      console.log(`Connected to SignalR for doc ${documentId}`);

      // Send join request
      socket.send(JSON.stringify({
        target: 'JoinDocumentGroup',
        arguments: [documentId],
      }));

      // Apply an operation
      socket.send(JSON.stringify({
        target: 'ApplyOperation',
        arguments: [{
          documentId: documentId,
          type: 'Insert',
          position: 5,
          content: 'Hello',
          version: 1,
          length: 5,
        }],
      }));
    });

    socket.on('message', function (msg) {
      console.log(`Received: ${msg}`);
    });

    socket.on('close', function () {
      console.log(`Disconnected from doc ${documentId}`);
    });

    sleep(1);
  });

  check(res, { 'SignalR connected': (r) => r && r.status === 101 });
}

export default function () {
  let documentId = createDocument();
  fetchDocument(documentId);
  testSignalR(documentId);
}
