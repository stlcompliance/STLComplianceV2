import React from "react";

export function NexarrSsoButton({ href = "/auth/nexarr/login" }: { href?: string }) {
  return (
    <a
      href={href}
      aria-label="Continue with Nexarr"
      className="inline-flex min-h-[52px] w-[336px] items-center justify-center gap-3 rounded-[13px] border border-[#2097ff]/80 bg-gradient-to-b from-[#041739] to-[#022f6e] px-5 font-semibold tracking-[0.01em] text-[#f5faff] no-underline shadow-lg shadow-blue-950/20 transition hover:-translate-y-0.5 hover:border-[#4ab2ff] hover:shadow-xl focus-visible:outline focus-visible:outline-4 focus-visible:outline-offset-4 focus-visible:outline-[#0e92ff]/35"
    >
      <img src="/assets/nexarr-mark.png" alt="" className="h-[34px] w-[49px] object-contain" />
      <span>Continue with Nexarr</span>
    </a>
  );
}
