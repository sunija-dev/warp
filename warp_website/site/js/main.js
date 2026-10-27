// Give the header a background once the page is scrolled.
const header = document.querySelector('.site-header');
const updateHeader = () => header.classList.toggle('scrolled', window.scrollY > 40);
window.addEventListener('scroll', updateHeader, { passive: true });
updateHeader();

// Fade sections in as they scroll into view. Added from JS so nothing stays hidden without it.
const revealObserver = new IntersectionObserver(entries => {
  entries.forEach(entry => {
    if (!entry.isIntersecting) return;
    entry.target.classList.add('visible');
    revealObserver.unobserve(entry.target);
  });
}, { threshold: 0.15 });
document.querySelectorAll('.feature, .howto-row > *, .showcase-row, .community .container, .page-content .shot')
  .forEach(el => {
    // Stagger side-by-side elements, e.g. the three feature columns
    if (el.matches('.feature, .howto-row > *'))
      el.style.transitionDelay = `${[...el.parentElement.children].indexOf(el) * 0.12}s`;
    el.classList.add('reveal');
    revealObserver.observe(el);
  });

// Replace the trailer placeholder with the YouTube player on click.
document.querySelectorAll('[data-youtube]').forEach(button => {
  button.addEventListener('click', () => {
    const iframe = document.createElement('iframe');
    iframe.className = 'trailer';
    iframe.src = `https://www.youtube-nocookie.com/embed/${button.dataset.youtube}?autoplay=1`;
    iframe.title = 'WARP trailer';
    iframe.allow = 'autoplay; encrypted-media; picture-in-picture; fullscreen';
    iframe.allowFullscreen = true;
    button.replaceWith(iframe);
  });
});

// Click a screenshot to open it full size; click again to close.
const dialog = document.createElement('dialog');
dialog.className = 'lightbox';
dialog.addEventListener('click', () => dialog.close());
document.body.append(dialog);
document.querySelectorAll('.shot img').forEach(img => {
  img.addEventListener('click', () => {
    dialog.replaceChildren(img.cloneNode());
    dialog.showModal();
  });
});
