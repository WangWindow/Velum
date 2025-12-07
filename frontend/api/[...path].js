export const config = {
    api: {
        bodyParser: false,
        externalResolver: true,
    },
};

export default async function handler(req, res) {
    const backendUrl = process.env.BACKEND_URL;

    if (!backendUrl) {
        res.status(500).json({ error: 'BACKEND_URL environment variable is not set' });
        return;
    }

    // Ensure backendUrl doesn't have a trailing slash
    const baseUrl = backendUrl.replace(/\/$/, '');

    // req.url includes the path and query string (e.g., /api/users?id=1)
    // We want to forward this to the backend
    const targetUrl = `${baseUrl}${req.url}`;

    try {
        const response = await fetch(targetUrl, {
            method: req.method,
            headers: {
                ...req.headers,
                host: new URL(baseUrl).host, // Update host header to match backend
                // Remove headers that might cause issues
                'x-forwarded-host': undefined,
                'x-forwarded-proto': undefined,
                'x-vercel-id': undefined,
            },
            body: ['GET', 'HEAD'].includes(req.method) ? undefined : req,
            duplex: 'half', // Required for streaming body in Node.js fetch
        });

        // Set status
        res.status(response.status);

        // Forward headers
        response.headers.forEach((value, key) => {
            // Skip some headers that might be handled by Vercel automatically or cause issues
            if (['content-encoding', 'content-length', 'transfer-encoding'].includes(key.toLowerCase())) {
                return;
            }
            res.setHeader(key, value);
        });

        // Stream the response body
        const reader = response.body.getReader();
        while (true) {
            const { done, value } = await reader.read();
            if (done) break;
            res.write(value);
        }
        res.end();

    } catch (error) {
        console.error('Proxy error:', error);
        res.status(500).json({ error: 'Error forwarding request to backend', details: error.message });
    }
}
