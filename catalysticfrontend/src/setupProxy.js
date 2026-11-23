const { createProxyMiddleware } = require("http-proxy-middleware");

// Development only: the local ASP.NET development certificate is self-signed.
module.exports = function setupProxy(app) {
  app.use("/api", createProxyMiddleware({
    target: "https://localhost:7216",
    changeOrigin: true,
    secure: false,
  }));
};
