const products = [
    { id: 1, name: 'Pulse X Wireless Headset', category: 'Accessories', price: 79.99, old: 99.99, image: '/website/images/product-headset.svg', tag: 'NEW' },
    { id: 2, name: 'PlayStation 5 Slim', category: 'Playstation', price: 499.99, old: 549.99, image: '/website/images/product-console.svg', tag: 'HOT' },
    { id: 3, name: 'Nebula RGB Mechanical Keyboard', category: 'PC Gaming', price: 89.99, old: 109.99, image: '/website/images/product-keyboard.svg', tag: 'SALE' },
    { id: 4, name: 'Stealth Pro Gaming Mouse', category: 'Accessories', price: 54.99, old: 69.99, image: '/website/images/product-mouse.svg', tag: 'SALE' },
    { id: 5, name: 'Xbox Wireless Controller', category: 'XBOX', price: 59.99, old: 64.99, image: '/website/images/product-controller.svg', tag: 'TOP' },
    { id: 6, name: 'Nintendo Switch OLED', category: 'Nintendo', price: 349.99, old: 379.99, image: '/website/images/product-switch.svg', tag: 'NEW' },
    { id: 7, name: 'NovaStream 27" Gaming Monitor', category: 'PC Gaming', price: 249.99, old: 299.99, image: '/website/images/product-monitor.svg', tag: 'DEAL' },
    { id: 8, name: 'GalaxyPad Pro Tablet', category: 'Phones & Tablets', price: 429.99, old: 479.99, image: '/website/images/product-tablet.svg', tag: 'NEW' }
];

let activeCategory = 'All', query = '', wishlist = new Set(), cart = [];

const $ = s => document.querySelector(s);
const categoryList = $('#categoryList');
const filters = $('#filters');
const grid = $('#productGrid');
const empty = $('#emptyState');

function money(n) { return '$' + n.toFixed(2); }

function renderFilters() {
    // Dynamic filter buttons based on actual products
    const uniqueCats = ['All', ...new Set(products.map(p => p.category))];
    filters.innerHTML = uniqueCats.map(c =>
        `<button class="filter-btn ${activeCategory === c ? 'active' : ''}" data-filter="${c}">${c}</button>`
    ).join('');
}

function productMatches(p) {
    const text = (p.name + ' ' + p.category + ' ' + p.tag).toLowerCase();
    const matchesCat = (activeCategory === 'All' || p.category.toLowerCase() === activeCategory.toLowerCase());
    return matchesCat && text.includes(query.toLowerCase());
}

function renderProducts() {
    const visible = products.filter(productMatches);
    empty.style.display = visible.length ? 'none' : 'block';
    grid.style.display = visible.length ? 'grid' : 'none';

    grid.innerHTML = visible.map(p => `
    <article class="product-card">
      <div class="product-image">
        <img src="${p.image}" alt="${p.name}">
        <span class="product-tag">${p.tag}</span>
        <button class="heart-btn ${wishlist.has(p.id) ? 'active' : ''}" data-wish="${p.id}" aria-label="Wishlist">
          <i class="${wishlist.has(p.id) ? 'fas fa-heart' : 'far fa-heart'}"></i>
        </button>
      </div>
      <div class="product-info">
        <div>
          <div class="product-category">${p.category}</div>
          <div class="product-name">${p.name}</div>
        </div>
        <div class="product-bottom">
          <div class="price">${money(p.price)} <span class="old-price">${money(p.old)}</span></div>
          <button class="add-btn" data-add="${p.id}">ADD TO CART</button>
        </div>
      </div>
    </article>
  `).join('');
}

function updateCounters() {
    $('#wishlistCount').textContent = wishlist.size;
    $('#cartCount').textContent = cart.length;
    $('#cartItemsText').textContent = cart.length ? `${cart.length} item(s) in your cart.` : 'Your cart is empty.';
    $('#cartTotal').textContent = cart.length ? 'Subtotal: ' + money(cart.reduce((s, p) => s + p.price, 0)) : '';
}

function toast(message) {
    const el = $('#toast');
    el.textContent = message;
    el.classList.add('show');
    clearTimeout(window.toastTimer);
    window.toastTimer = setTimeout(() => el.classList.remove('show'), 2000);
}

function setCategory(c) {
    activeCategory = c;

    // Highlight selected button inside the Razor-rendered category sidebar
    document.querySelectorAll('.category-item').forEach(btn => {
        const isMatch = btn.dataset.category?.toLowerCase() === c.toLowerCase();
        btn.style.background = isMatch ? '#eef2ff' : 'none';
        btn.style.color = isMatch ? '#4f46e5' : '#1e293b';
    });

    renderFilters();
    renderProducts();
}

// Category sidebar click event listener
categoryList?.addEventListener('click', e => {
    const b = e.target.closest('[data-category]');
    if (b) {
        setCategory(b.dataset.category);
        document.getElementById('products')?.scrollIntoView({ behavior: 'smooth' });
    }
});

// Top filter buttons click event listener
filters?.addEventListener('click', e => {
    const b = e.target.closest('[data-filter]');
    if (b) setCategory(b.dataset.filter);
});

// Grid actions (Wishlist & Cart)
grid?.addEventListener('click', e => {
    const wish = e.target.closest('[data-wish]');
    const add = e.target.closest('[data-add]');

    if (wish) {
        const id = +wish.dataset.wish;
        wishlist.has(id) ? wishlist.delete(id) : wishlist.add(id);
        updateCounters();
        renderProducts();
        toast(wishlist.has(id) ? 'Added to wishlist' : 'Removed from wishlist');
    }

    if (add) {
        const id = +add.dataset.add;
        cart.push(products.find(p => p.id === id));
        updateCounters();
        toast('Added to cart');
    }
});

$('#searchInput')?.addEventListener('input', e => {
    query = e.target.value;
    renderProducts();
});

$('#clearSearch')?.addEventListener('click', () => {
    $('#searchInput').value = '';
    query = '';
    renderProducts();
});

// Hero Slider
let slide = 0;
const slides = document.querySelectorAll('.hero-slide');
function showSlide(i) {
    slide = (i + slides.length) % slides.length;
    slides.forEach((s, n) => s.classList.toggle('active', n === slide));
}
if (slides.length > 0) {
    setInterval(() => showSlide(slide + 1), 5000);
}

// Smooth scroll buttons
document.querySelectorAll('[data-scroll]').forEach(b => {
    b.addEventListener('click', () => {
        document.getElementById(b.dataset.scroll)?.scrollIntoView({ behavior: 'smooth' });
    });
});

// Cart and Toast Drawers
$('#cartBtn').onclick = () => $('#miniCart').classList.add('open');
$('#closeCart').onclick = () => $('#miniCart').classList.remove('open');
$('#viewMore').onclick = () => { setCategory('All'); toast('Showing all products'); };
$('#signupBtn').onclick = () => toast('Welcome to Molla Club!');

$('#emailBtn').onclick = () => {
    const email = $('#emailInput').value.trim();
    if (email.includes('@')) {
        toast('Thanks! You are on the VIP list.');
        $('#emailInput').value = '';
    } else {
        toast('Please enter a valid email.');
    }
};

// Initial Load
renderFilters();
renderProducts();
updateCounters();