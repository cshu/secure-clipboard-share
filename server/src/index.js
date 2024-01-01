// src/templates/populated-worker/src/index.js

async function digestMessage(message) {
  const msgUint8 = new TextEncoder().encode(message);
  const hashBuffer = await crypto.subtle.digest("SHA-256", msgUint8);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  const hashHex = hashArray
    .map((b) => b.toString(16).padStart(2, "0"))
    .join("");
  return hashHex;
}

//import renderHtml from "./renderHtml.js";
var src_default = {
  async fetch(request, env) {
    const { DATABASE } = env;
    if (request.method !== 'POST') {
      return new Response('', {status: 200});
    }
    const body = await request.text();
    let rbody = JSON.parse(body);
    let hashedkey = await digestMessage(rbody.k);
    //const stmt = await DATABASE.prepare("SELECT * FROM defkv where k = ?").bind(hashedkey);
    //const { results } = await stmt.all();
    if (undefined === rbody.v) {
      const results = await DATABASE.prepare("SELECT v FROM defkv where k = ?").bind(hashedkey).all();
      return new Response(JSON.stringify(results));
    }
    const uresults = await DATABASE.prepare("update defkv set v = ? where k = ?").bind(rbody.v, hashedkey).all();
    return new Response(JSON.stringify(uresults));
  }
};
export {
  src_default as default
};
