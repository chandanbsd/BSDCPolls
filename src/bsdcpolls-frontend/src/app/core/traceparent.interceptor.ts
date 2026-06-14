import { HttpInterceptorFn } from '@angular/common/http';

/**
 * HTTP interceptor that injects a W3C traceparent header into every outbound
 * BFF request. The trace ID is derived from the current timestamp to produce
 * a unique per-request identifier for end-to-end correlation in SigNoz.
 */
export const traceparentInterceptor: HttpInterceptorFn = (req, next) => {
  const traceId = generateTraceId();
  const spanId = generateSpanId();
  const traceparent = `00-${traceId}-${spanId}-01`;

  const cloned = req.clone({
    headers: req.headers.set('traceparent', traceparent),
  });

  return next(cloned);
};

function generateTraceId(): string {
  return Array.from(crypto.getRandomValues(new Uint8Array(16)))
    .map((b) => b.toString(16).padStart(2, '0'))
    .join('');
}

function generateSpanId(): string {
  return Array.from(crypto.getRandomValues(new Uint8Array(8)))
    .map((b) => b.toString(16).padStart(2, '0'))
    .join('');
}
