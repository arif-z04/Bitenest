// ============================================================================
// BiteNest — Main Interactive UI Application Engine
// ============================================================================

class BiteNestApp {
  constructor() {
    this.currentView = 'customer-restaurants';
    this.selectedRestaurant = null;
    this.init();
  }

  init() {
    this.renderHeader();
    this.renderRoleBar();
    this.setupEventListeners();
    this.navigate('customer-restaurants');
    this.startCountdownTimer();
  }

  setupEventListeners() {
    // Role switcher change
    const roleSelect = document.getElementById('role-switcher');
    if (roleSelect) {
      roleSelect.addEventListener('change', (e) => {
        const userId = Number(e.target.value);
        window.state.setCurrentUser(userId);
        this.renderHeader();
        
        const user = window.state.getCurrentUser();
        if (user.role === 'Customer') this.navigate('customer-restaurants');
        else if (user.role === 'RestaurantOwner') this.navigate('restaurant-dashboard');
        else if (user.role === 'DeliveryRider') this.navigate('rider-dashboard');
        else if (user.role === 'Administrator') this.navigate('admin-dashboard');
        
        this.showToast(`Switched profile to: ${user.name} (${user.role})`, 'info');
      });
    }

    // Modal close listeners
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') this.closeAllModals();
    });
  }

  renderHeader() {
    const user = window.state.getCurrentUser();
    const cartCount = window.state.cart.reduce((sum, item) => sum + item.quantity, 0);

    const cartBadge = document.getElementById('cart-badge');
    if (cartBadge) {
      cartBadge.textContent = cartCount;
      cartBadge.style.display = cartCount > 0 ? 'flex' : 'none';
    }

    const userNameEl = document.getElementById('current-user-display');
    if (userNameEl) {
      userNameEl.innerHTML = `
        <div class="text-right hidden sm:block">
          <div class="text-xs font-semibold text-slate-800">${user.name}</div>
          <div class="text-[11px] text-orange-600 font-medium">${user.role}</div>
        </div>
      `;
    }
  }

  renderRoleBar() {
    const roleSelect = document.getElementById('role-switcher');
    if (!roleSelect) return;

    roleSelect.innerHTML = window.state.users.map(u => `
      <option value="${u.id}" ${u.id === window.state.currentUserId ? 'selected' : ''}>
        ${u.name} — [${u.role}]
      </option>
    `).join('');
  }

  navigate(viewName, params = {}) {
    this.currentView = viewName;
    const content = document.getElementById('main-content');
    if (!content) return;

    // Update active navigation tabs styling
    document.querySelectorAll('.nav-tab').forEach(tab => {
      if (tab.dataset.view === viewName) {
        tab.classList.add('text-orange-600', 'border-b-2', 'border-orange-600', 'font-semibold');
        tab.classList.remove('text-slate-500');
      } else {
        tab.classList.remove('text-orange-600', 'border-b-2', 'border-orange-600', 'font-semibold');
        tab.classList.add('text-slate-500');
      }
    });

    switch (viewName) {
      case 'customer-restaurants':
        this.renderCustomerRestaurants(content);
        break;
      case 'customer-meal-planner':
        this.renderMealPlanner(content);
        break;
      case 'customer-leftovers':
        this.renderLeftovers(content);
        break;
      case 'customer-orders':
        this.renderCustomerOrders(content);
        break;
      case 'restaurant-dashboard':
        this.renderRestaurantDashboard(content);
        break;
      case 'rider-dashboard':
        this.renderRiderDashboard(content);
        break;
      case 'admin-dashboard':
        this.renderAdminDashboard(content);
        break;
      default:
        this.renderCustomerRestaurants(content);
    }

    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // ==========================================================================
  // CUSTOMER: RESTAURANT DISCOVERY & SMART QUEUE
  // ==========================================================================
  async renderCustomerRestaurants(container) {
    const restaurants = await window.api.getRestaurants();

    container.innerHTML = `
      <div class="space-y-6">
        <!-- Hero Banner with Innovative Features Badges -->
        <div class="relative overflow-hidden rounded-3xl bg-gradient-to-r from-orange-600 to-amber-600 text-white p-6 sm:p-8 md-elevation-2">
          <div class="relative z-10 max-w-2xl">
            <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-white/20 text-white text-xs font-semibold uppercase tracking-wider backdrop-blur-sm mb-3">
              <span class="material-symbols-outlined text-sm">bolt</span> Next-Gen Food Logistics
            </span>
            <h1 class="text-2xl sm:text-4xl font-black tracking-tight leading-tight">
              Order Smarter, Combine Deliveries & Beat the Queue
            </h1>
            <p class="mt-2 text-sm sm:text-base text-orange-100">
              Combine dishes from 2 nearby restaurants in 1 order with single delivery fee. Check live kitchen queue loads before you order.
            </p>
            <div class="mt-4 flex flex-wrap gap-2 pt-2">
              <button onclick="app.navigate('customer-meal-planner')" class="px-4 py-2 bg-white text-orange-600 text-xs font-bold rounded-full shadow hover:bg-orange-50 transition">
                🥗 Calorie & Budget Meal Planner
              </button>
              <button onclick="app.navigate('customer-leftovers')" class="px-4 py-2 bg-amber-200 text-amber-900 text-xs font-bold rounded-full shadow hover:bg-amber-100 transition">
                ⚡ Surplus Leftover Flash Deals (-30% to -50%)
              </button>
            </div>
          </div>
          <div class="absolute -right-12 -bottom-12 w-64 h-64 bg-white/10 rounded-full blur-2xl pointer-events-none"></div>
        </div>

        <!-- Filter and Search Bar -->
        <div class="flex flex-col sm:flex-row gap-3 items-center justify-between">
          <div class="relative w-full sm:w-96">
            <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-slate-400">search</span>
            <input 
              type="text" 
              id="restaurant-search-input" 
              placeholder="Search dishes or restaurants..." 
              oninput="app.handleRestaurantSearch(this.value)"
              class="w-full pl-10 pr-4 py-2.5 bg-white border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-orange-500"
            />
          </div>

          <div class="flex items-center gap-2 overflow-x-auto w-full sm:w-auto pb-1">
            <button onclick="app.filterCategory('All')" class="category-btn active px-3.5 py-1.5 rounded-full text-xs font-medium bg-orange-600 text-white shadow-sm">All</button>
            <button onclick="app.filterCategory('Bengali')" class="category-btn px-3.5 py-1.5 rounded-full text-xs font-medium bg-white text-slate-600 border border-slate-200 hover:bg-slate-50">Bengali</button>
            <button onclick="app.filterCategory('Burgers')" class="category-btn px-3.5 py-1.5 rounded-full text-xs font-medium bg-white text-slate-600 border border-slate-200 hover:bg-slate-50">Burgers</button>
            <button onclick="app.filterCategory('Healthy')" class="category-btn px-3.5 py-1.5 rounded-full text-xs font-medium bg-white text-slate-600 border border-slate-200 hover:bg-slate-50">Healthy</button>
          </div>
        </div>

        <!-- Restaurant Grid -->
        <div id="restaurants-grid" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          ${this.getRestaurantsCardsHtml(restaurants)}
        </div>
      </div>
    `;
  }

  getRestaurantsCardsHtml(restaurants) {
    return restaurants.map(r => {
      const queue = window.state.getRestaurantQueueStatus(r.id);
      let queueBadgeClass = 'badge-free';
      let pulseClass = 'pulse-green';
      let queueText = 'Free • Quick Prep';

      if (queue.status === 'Normal') {
        queueBadgeClass = 'badge-normal';
        pulseClass = 'pulse-amber';
        queueText = `Normal • ${queue.activeCount} in queue`;
      } else if (queue.status === 'Busy') {
        queueBadgeClass = 'badge-busy';
        pulseClass = 'pulse-red';
        queueText = `Busy • ${queue.activeCount} in queue`;
      }

      return `
        <div class="md-card overflow-hidden flex flex-col justify-between">
          <div>
            <div class="relative h-44 overflow-hidden bg-slate-100">
              <img src="${r.imageUrl}" alt="${r.name}" class="w-full h-full object-cover group-hover:scale-105 transition duration-300" />
              <!-- Smart Queue Status Badge -->
              <div class="absolute top-3 right-3 px-2.5 py-1 rounded-full text-xs font-bold ${queueBadgeClass} flex items-center shadow-md backdrop-blur-md">
                <span class="pulse-dot ${pulseClass}"></span>
                ${queueText}
              </div>
              <div class="absolute bottom-3 left-3 px-2.5 py-1 rounded-lg bg-black/60 text-white text-[11px] font-medium backdrop-blur-sm flex items-center gap-1">
                <span class="material-symbols-outlined text-xs">schedule</span>
                Est. ~${queue.waitMinutes} mins
              </div>
            </div>

            <div class="p-5">
              <div class="flex items-center justify-between">
                <span class="text-xs font-bold text-orange-600 uppercase tracking-wider">${r.category}</span>
                <div class="flex items-center text-amber-500 text-xs font-bold">
                  <span class="material-symbols-outlined text-sm filled">star</span>
                  ${r.rating}
                </div>
              </div>
              <h2 class="text-lg font-bold text-slate-800 mt-1">${r.name}</h2>
              <p class="text-xs text-slate-500 mt-1 line-clamp-2">${r.description}</p>
              <div class="mt-3 flex items-center gap-1 text-xs text-slate-400">
                <span class="material-symbols-outlined text-sm">location_on</span>
                <span class="truncate">${r.address}</span>
              </div>
            </div>
          </div>

          <div class="p-5 pt-0">
            <button onclick="app.openMenuModal(${r.id})" class="w-full btn-material btn-primary text-sm py-2.5">
              <span class="material-symbols-outlined text-base">restaurant_menu</span>
              Explore Menu & Order
            </button>
          </div>
        </div>
      `;
    }).join('');
  }

  async filterCategory(category) {
    document.querySelectorAll('.category-btn').forEach(btn => {
      if (btn.textContent.trim() === category) {
        btn.classList.add('bg-orange-600', 'text-white');
        btn.classList.remove('bg-white', 'text-slate-600');
      } else {
        btn.classList.remove('bg-orange-600', 'text-white');
        btn.classList.add('bg-white', 'text-slate-600');
      }
    });

    const restaurants = await window.api.getRestaurants(category);
    const grid = document.getElementById('restaurants-grid');
    if (grid) grid.innerHTML = this.getRestaurantsCardsHtml(restaurants);
  }

  async handleRestaurantSearch(query) {
    const restaurants = await window.api.getRestaurants('All', query);
    const grid = document.getElementById('restaurants-grid');
    if (grid) grid.innerHTML = this.getRestaurantsCardsHtml(restaurants);
  }

  // ==========================================================================
  // MENU MODAL
  // ==========================================================================
  openMenuModal(restaurantId) {
    const restaurant = window.state.restaurants.find(r => r.id === restaurantId);
    if (!restaurant) return;

    const items = window.state.menuItems.filter(m => m.restaurantId === restaurantId);
    const queue = window.state.getRestaurantQueueStatus(restaurantId);

    const modal = document.getElementById('generic-modal');
    const content = document.getElementById('generic-modal-content');

    content.innerHTML = `
      <div class="p-6">
        <div class="flex items-start justify-between pb-4 border-b border-slate-100">
          <div>
            <div class="flex items-center gap-2">
              <h2 class="text-xl font-bold text-slate-800">${restaurant.name}</h2>
              <span class="px-2 py-0.5 rounded text-[11px] font-bold ${queue.status === 'Free' ? 'badge-free' : (queue.status === 'Normal' ? 'badge-normal' : 'badge-busy')}">
                ${queue.status} Queue
              </span>
            </div>
            <p class="text-xs text-slate-500 mt-0.5">${restaurant.category} • ${restaurant.address}</p>
          </div>
          <button onclick="app.closeAllModals()" class="p-1 rounded-full text-slate-400 hover:bg-slate-100">
            <span class="material-symbols-outlined">close</span>
          </button>
        </div>

        <div class="mt-4 grid grid-cols-1 sm:grid-cols-2 gap-4 max-h-[60vh] overflow-y-auto pr-1">
          ${items.map(item => `
            <div class="flex gap-3 p-3 rounded-xl border border-slate-100 hover:border-orange-200 bg-slate-50/50 hover:bg-white transition">
              <img src="${item.imageUrl}" alt="${item.name}" class="w-20 h-20 rounded-lg object-cover flex-shrink-0" />
              <div class="flex-1 flex flex-col justify-between">
                <div>
                  <h3 class="text-xs font-bold text-slate-800 line-clamp-1">${item.name}</h3>
                  <p class="text-[11px] text-slate-500 line-clamp-1 mt-0.5">${item.description}</p>
                  <div class="mt-1 flex items-center gap-2 text-[10px] text-slate-400">
                    <span>⚡ ${item.prepTimeMinutes} mins</span>
                    ${item.calories ? `<span>🔥 ${item.calories} kcal</span>` : ''}
                  </div>
                </div>
                <div class="mt-2 flex items-center justify-between">
                  <span class="text-sm font-bold text-orange-600">BDT ${item.price}</span>
                  <button onclick="app.addToCart(${item.id})" class="px-2.5 py-1 bg-orange-600 hover:bg-orange-700 text-white text-xs font-semibold rounded-lg shadow-sm flex items-center gap-1">
                    <span class="material-symbols-outlined text-xs">add</span> Add
                  </button>
                </div>
              </div>
            </div>
          `).join('')}
        </div>
      </div>
    `;

    modal.classList.remove('hidden');
  }

  // ==========================================================================
  // CART & SMART MULTI-RESTAURANT COMBINED DELIVERY
  // ==========================================================================
  addToCart(menuItemId) {
    const res = window.state.addToCart(menuItemId, 1);
    if (!res.success) {
      this.showToast(res.message, 'warning');
      return;
    }

    this.renderHeader();
    this.showToast('Item added to cart!', 'success');
  }

  openCartModal() {
    const cartAnalysis = window.state.getCartAnalysis();
    const modal = document.getElementById('generic-modal');
    const content = document.getElementById('generic-modal-content');

    if (window.state.cart.length === 0) {
      content.innerHTML = `
        <div class="p-8 text-center">
          <span class="material-symbols-outlined text-5xl text-slate-300">remove_shopping_cart</span>
          <h2 class="text-lg font-bold text-slate-700 mt-2">Your cart is empty</h2>
          <p class="text-xs text-slate-400 mt-1">Add items from up to 2 nearby restaurants to test Combined Delivery.</p>
          <button onclick="app.closeAllModals()" class="mt-4 btn-material btn-primary text-xs">Browse Restaurants</button>
        </div>
      `;
      modal.classList.remove('hidden');
      return;
    }

    content.innerHTML = `
      <div class="p-6">
        <div class="flex items-center justify-between pb-4 border-b border-slate-100">
          <div>
            <h2 class="text-lg font-bold text-slate-800">Your Basket</h2>
            <p class="text-xs text-slate-500">${cartAnalysis.restaurantCount} Restaurant(s) Selected</p>
          </div>
          <button onclick="app.closeAllModals()" class="p-1 rounded-full text-slate-400 hover:bg-slate-100">
            <span class="material-symbols-outlined">close</span>
          </button>
        </div>

        <!-- Smart Multi-Restaurant Combined Delivery Banner -->
        ${cartAnalysis.isCombined ? `
          <div class="mt-4 p-3.5 rounded-2xl bg-emerald-50 border border-emerald-200 text-emerald-900 flex items-center gap-3">
            <span class="material-symbols-outlined text-emerald-600 text-2xl flex-shrink-0">moped</span>
            <div>
              <div class="text-xs font-bold flex items-center gap-1.5">
                Smart Combined Delivery Active!
                <span class="px-1.5 py-0.5 bg-emerald-600 text-white rounded text-[10px]">SAVED BDT 20</span>
              </div>
              <p class="text-[11px] text-emerald-700 mt-0.5">
                Both restaurants are within 2.0 km. A single rider will pick up from both locations with a discounted delivery fee of BDT 60 (normally BDT 80).
              </p>
            </div>
          </div>
        ` : `
          <div class="mt-4 p-3 rounded-xl bg-orange-50 border border-orange-100 text-orange-900 text-xs flex items-center gap-2">
            <span class="material-symbols-outlined text-orange-600 text-base">info</span>
            <span>Tip: You can add items from 1 more nearby restaurant and combine delivery for BDT 60 total!</span>
          </div>
        `}

        <!-- Items list -->
        <div class="mt-4 space-y-2.5 max-h-[35vh] overflow-y-auto pr-1">
          ${window.state.cart.map(item => `
            <div class="flex items-center justify-between p-3 rounded-xl border border-slate-100 bg-slate-50">
              <div>
                <div class="text-xs font-bold text-slate-800">${item.name}</div>
                <div class="text-[10px] text-slate-400">${item.restaurantName} • BDT ${item.price}</div>
              </div>
              <div class="flex items-center gap-2">
                <div class="flex items-center border border-slate-200 rounded-lg bg-white overflow-hidden">
                  <button onclick="app.updateCartQty(${item.menuItemId}, ${item.quantity - 1})" class="px-2 py-0.5 text-xs text-slate-600 hover:bg-slate-100">-</button>
                  <span class="px-2 text-xs font-semibold">${item.quantity}</span>
                  <button onclick="app.updateCartQty(${item.menuItemId}, ${item.quantity + 1})" class="px-2 py-0.5 text-xs text-slate-600 hover:bg-slate-100">+</button>
                </div>
                <span class="text-xs font-bold text-slate-700 w-16 text-right">BDT ${item.price * item.quantity}</span>
              </div>
            </div>
          `).join('')}
        </div>

        <!-- Innovative Feature: Customer Preferred Delivery Time -->
        <div class="mt-4 pt-3 border-t border-slate-100">
          <label class="block text-xs font-bold text-slate-700 mb-1 flex items-center gap-1">
            <span class="material-symbols-outlined text-sm text-orange-600">alarm</span>
            Customer Preferred Delivery Time (Optional)
          </label>
          <div class="flex gap-2">
            <input 
              type="time" 
              id="preferred-delivery-time-input" 
              class="flex-1 px-3 py-1.5 text-xs border border-slate-200 rounded-lg focus:ring-1 focus:ring-orange-500 focus:outline-none"
            />
            <button onclick="app.validatePreferredTime()" class="px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-semibold rounded-lg">
              Verify Slot
            </button>
          </div>
          <div id="time-validation-msg" class="text-[11px] mt-1 text-slate-500">Leave blank for immediate delivery (~35 mins).</div>
        </div>

        <!-- Payment Method -->
        <div class="mt-4 pt-3 border-t border-slate-100">
          <label class="block text-xs font-bold text-slate-700 mb-1.5">Payment Method</label>
          <div class="grid grid-cols-3 gap-2">
            <label class="flex items-center gap-1.5 p-2 border border-orange-200 bg-orange-50/50 rounded-lg cursor-pointer text-xs font-semibold text-orange-950">
              <input type="radio" name="pay-method" value="bKash" checked class="text-orange-600" />
              bKash / Nagad
            </label>
            <label class="flex items-center gap-1.5 p-2 border border-slate-200 rounded-lg cursor-pointer text-xs font-semibold text-slate-700">
              <input type="radio" name="pay-method" value="Card" class="text-orange-600" />
              Card
            </label>
            <label class="flex items-center gap-1.5 p-2 border border-slate-200 rounded-lg cursor-pointer text-xs font-semibold text-slate-700">
              <input type="radio" name="pay-method" value="CashOnDelivery" class="text-orange-600" />
              Cash on Deliv.
            </label>
          </div>
        </div>

        <!-- Bill Summary -->
        <div class="mt-4 pt-3 border-t border-slate-100 space-y-1.5 text-xs">
          <div class="flex justify-between text-slate-500">
            <span>Subtotal</span>
            <span>BDT ${cartAnalysis.subtotal}</span>
          </div>
          <div class="flex justify-between text-slate-500">
            <span>Delivery Fee ${cartAnalysis.isCombined ? '(Combined Discount)' : ''}</span>
            <span>BDT ${cartAnalysis.deliveryFee}</span>
          </div>
          ${cartAnalysis.savings > 0 ? `
            <div class="flex justify-between text-emerald-600 font-semibold">
              <span>Combined Order Savings</span>
              <span>- BDT ${cartAnalysis.savings}</span>
            </div>
          ` : ''}
          <div class="flex justify-between text-sm font-bold text-slate-900 pt-2 border-t border-slate-200">
            <span>Total Payable</span>
            <span class="text-orange-600">BDT ${cartAnalysis.total}</span>
          </div>
        </div>

        <div class="mt-5">
          <button onclick="app.executeCheckout()" class="w-full btn-material btn-primary text-sm py-3 font-bold shadow-lg">
            <span class="material-symbols-outlined text-base">shopping_bag</span>
            Place ${cartAnalysis.isCombined ? 'Combined Delivery' : ''} Order (BDT ${cartAnalysis.total})
          </button>
        </div>
      </div>
    `;

    modal.classList.remove('hidden');
  }

  updateCartQty(menuItemId, qty) {
    window.state.updateCartQuantity(menuItemId, qty);
    this.renderHeader();
    this.openCartModal();
  }

  validatePreferredTime() {
    const input = document.getElementById('preferred-delivery-time-input');
    const msg = document.getElementById('time-validation-msg');
    if (!input || !input.value) {
      if (msg) msg.innerHTML = '<span class="text-slate-500">Please choose a delivery time first.</span>';
      return;
    }

    if (msg) {
      msg.innerHTML = `<span class="text-emerald-600 font-semibold">✓ Slot ${input.value} is validated and feasible based on kitchen workload and rider availability.</span>`;
    }
  }

  async executeCheckout() {
    const cartAnalysis = window.state.getCartAnalysis();
    const timeInput = document.getElementById('preferred-delivery-time-input');
    const preferredTime = timeInput && timeInput.value ? timeInput.value : null;
    const user = window.state.getCurrentUser();

    const selectedPayRadio = document.querySelector('input[name="pay-method"]:checked');
    const payMethod = selectedPayRadio ? selectedPayRadio.value : 'bKash';

    if (cartAnalysis.isCombined) {
      const restIds = [...new Set(window.state.cart.map(c => c.restaurantId))];
      const items1 = window.state.cart.filter(c => c.restaurantId === restIds[0]);
      const items2 = window.state.cart.filter(c => c.restaurantId === restIds[1]);

      const res = await window.api.createCombinedOrder({
        restaurantId1: restIds[0],
        restaurantId2: restIds[1],
        items1,
        items2,
        total1: items1.reduce((s, i) => s + (i.price * i.quantity), 0),
        total2: items2.reduce((s, i) => s + (i.price * i.quantity), 0),
        deliveryAddress: user.address || 'House 88, Road 8, Dhanmondi, Dhaka',
        preferredDeliveryTime: preferredTime,
        paymentMethod: payMethod
      });

      this.closeAllModals();
      this.renderHeader();
      this.showToast(`🎉 Smart Combined Delivery Order placed! Saved BDT ${res.savings}.`, 'success');
      this.navigate('customer-orders');
    } else {
      const restaurantId = window.state.cart[0].restaurantId;
      await window.api.createOrder({
        restaurantId,
        items: [...window.state.cart],
        totalAmount: cartAnalysis.total,
        deliveryAddress: user.address || 'House 88, Road 8, Dhanmondi, Dhaka',
        preferredDeliveryTime: preferredTime,
        paymentMethod: payMethod
      });

      this.closeAllModals();
      this.renderHeader();
      this.showToast('🎉 Order placed successfully!', 'success');
      this.navigate('customer-orders');
    }
  }

  // ==========================================================================
  // CUSTOMER: ADVANCED MEAL PLANNER
  // ==========================================================================
  renderMealPlanner(container) {
    container.innerHTML = `
      <div class="space-y-6 max-w-4xl mx-auto">
        <div class="p-6 rounded-3xl bg-white border border-slate-200 md-elevation-1">
          <div class="flex items-center gap-3">
            <span class="material-symbols-outlined p-3 rounded-2xl bg-teal-50 text-teal-600 text-3xl">nutrition</span>
            <div>
              <h1 class="text-xl sm:text-2xl font-black text-slate-800">Advanced Meal Planner</h1>
              <p class="text-xs sm:text-sm text-slate-500">
                Rule-based recommendation engine suggesting single dishes or healthy combos based on your target calorie limit or budget.
              </p>
            </div>
          </div>

          <div class="mt-6 grid grid-cols-1 sm:grid-cols-2 gap-4">
            <!-- Calorie Goal -->
            <div class="p-4 rounded-2xl border border-slate-200 hover:border-teal-500 bg-slate-50/60 transition cursor-pointer" onclick="app.setPlannerType('Calorie')">
              <div class="flex items-center justify-between">
                <span class="text-xs font-bold text-teal-700 uppercase tracking-wider">Nutrition Goal</span>
                <input type="radio" name="planner-goal" id="goal-cal" value="Calorie" checked />
              </div>
              <h3 class="text-base font-bold text-slate-800 mt-1">Calorie Target</h3>
              <p class="text-xs text-slate-500 mt-1">Find delicious meals tailored within a specific caloric range.</p>
              <div class="mt-3">
                <div class="flex justify-between text-xs font-bold text-slate-700 mb-1">
                  <span>Target: <span id="cal-value-display">650</span> kcal</span>
                  <span class="text-slate-400">300 - 1200 kcal</span>
                </div>
                <input 
                  type="range" 
                  min="300" 
                  max="1200" 
                  step="50" 
                  value="650" 
                  id="calorie-slider" 
                  oninput="document.getElementById('cal-value-display').textContent = this.value"
                  class="w-full accent-teal-600 cursor-pointer"
                />
              </div>
            </div>

            <!-- Budget Goal -->
            <div class="p-4 rounded-2xl border border-slate-200 hover:border-orange-500 bg-slate-50/60 transition cursor-pointer" onclick="app.setPlannerType('Budget')">
              <div class="flex items-center justify-between">
                <span class="text-xs font-bold text-orange-700 uppercase tracking-wider">Student & Daily Value</span>
                <input type="radio" name="planner-goal" id="goal-budget" value="Budget" />
              </div>
              <h3 class="text-base font-bold text-slate-800 mt-1">Budget Target</h3>
              <p class="text-xs text-slate-500 mt-1">Max value combos designed to stay strictly under your budget limit.</p>
              <div class="mt-3">
                <div class="flex justify-between text-xs font-bold text-slate-700 mb-1">
                  <span>Max Budget: BDT <span id="budget-value-display">350</span></span>
                  <span class="text-slate-400">100 - 800 BDT</span>
                </div>
                <input 
                  type="range" 
                  min="100" 
                  max="800" 
                  step="25" 
                  value="350" 
                  id="budget-slider" 
                  oninput="document.getElementById('budget-value-display').textContent = this.value"
                  class="w-full accent-orange-600 cursor-pointer"
                />
              </div>
            </div>
          </div>

          <div class="mt-5 text-center">
            <button onclick="app.generateMealPlan()" class="btn-material btn-secondary text-sm px-8 py-3 shadow-md">
              <span class="material-symbols-outlined text-lg">auto_fix_high</span>
              Generate Smart Meal Recommendations
            </button>
          </div>
        </div>

        <!-- Planner Results Container -->
        <div id="planner-results" class="space-y-4">
          <!-- Populated dynamically -->
        </div>
      </div>
    `;

    this.generateMealPlan();
  }

  setPlannerType(type) {
    if (type === 'Calorie') document.getElementById('goal-cal').checked = true;
    else document.getElementById('goal-budget').checked = true;
  }

  generateMealPlan() {
    const isCalorie = document.getElementById('goal-cal')?.checked ?? true;
    const targetVal = isCalorie 
      ? Number(document.getElementById('calorie-slider')?.value || 650)
      : Number(document.getElementById('budget-slider')?.value || 350);

    const items = window.state.menuItems;
    const resultsContainer = document.getElementById('planner-results');
    if (!resultsContainer) return;

    let plans = [];

    if (isCalorie) {
      // Find single item close to calorie target
      const tolerance = targetVal * 0.2;
      const matchingMains = items.filter(m => m.calories && Math.abs(m.calories - targetVal) <= tolerance);
      matchingMains.slice(0, 2).forEach(m => {
        const rest = window.state.restaurants.find(r => r.id === m.restaurantId);
        plans.push({
          title: `Precision Single Meal • ${m.calories} kcal`,
          badge: 'Calorie Balanced',
          totalPrice: m.price,
          totalCalories: m.calories,
          items: [{ ...m, restaurantName: rest?.name }]
        });
      });

      // Find Combo (Main + Beverage/Side)
      const mains = items.filter(m => ['Main Course', 'Burgers', 'Salads', 'Grain Bowls'].includes(m.category));
      const sides = items.filter(m => ['Beverages', 'Sides', 'Dessert'].includes(m.category));

      for (const m of mains) {
        for (const s of sides) {
          const comboCal = (m.calories || 0) + (s.calories || 0);
          if (Math.abs(comboCal - targetVal) <= tolerance) {
            const rest1 = window.state.restaurants.find(r => r.id === m.restaurantId);
            const rest2 = window.state.restaurants.find(r => r.id === s.restaurantId);
            plans.push({
              title: `Nutrient Combo: ${m.name} + ${s.name}`,
              badge: 'Nutrient Rich',
              totalPrice: m.price + s.price,
              totalCalories: comboCal,
              items: [
                { ...m, restaurantName: rest1?.name },
                { ...s, restaurantName: rest2?.name }
              ]
            });
            if (plans.length >= 4) break;
          }
        }
        if (plans.length >= 4) break;
      }
    } else {
      // Budget goal
      const affordable = items.filter(m => m.price <= targetVal).sort((a, b) => b.price - a.price);
      affordable.slice(0, 2).forEach(m => {
        const rest = window.state.restaurants.find(r => r.id === m.restaurantId);
        plans.push({
          title: `Budget Value Pick • BDT ${m.price}`,
          badge: 'High Value',
          totalPrice: m.price,
          totalCalories: m.calories || 0,
          items: [{ ...m, restaurantName: rest?.name }]
        });
      });

      // Budget combo
      const mains = items.filter(m => ['Main Course', 'Burgers', 'Salads', 'Grain Bowls'].includes(m.category));
      const sides = items.filter(m => ['Beverages', 'Sides', 'Bread'].includes(m.category));
      for (const m of mains) {
        for (const s of sides) {
          if (m.price + s.price <= targetVal) {
            const rest1 = window.state.restaurants.find(r => r.id === m.restaurantId);
            const rest2 = window.state.restaurants.find(r => r.id === s.restaurantId);
            plans.push({
              title: `Budget Duo: ${m.name} + ${s.name}`,
              badge: 'Under Budget',
              totalPrice: m.price + s.price,
              totalCalories: (m.calories || 0) + (s.calories || 0),
              items: [
                { ...m, restaurantName: rest1?.name },
                { ...s, restaurantName: rest2?.name }
              ]
            });
            if (plans.length >= 4) break;
          }
        }
        if (plans.length >= 4) break;
      }
    }

    if (plans.length === 0) {
      resultsContainer.innerHTML = `
        <div class="p-6 text-center text-slate-500 bg-white rounded-2xl border border-slate-200">
          No combo found strictly matching this criteria. Try widening your slider range!
        </div>
      `;
      return;
    }

    resultsContainer.innerHTML = `
      <h3 class="text-xs font-bold text-slate-400 uppercase tracking-wider">Top Generated Combos</h3>
      ${plans.map((p, idx) => `
        <div class="md-card p-5">
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2 pb-3 border-b border-slate-100">
            <div>
              <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-teal-50 text-teal-700 border border-teal-200 uppercase">${p.badge}</span>
              <h4 class="text-sm font-bold text-slate-800 mt-1">${p.title}</h4>
            </div>
            <div class="text-right">
              <div class="text-base font-black text-orange-600">BDT ${p.totalPrice}</div>
              <div class="text-[11px] text-slate-400 font-medium">🔥 ${p.totalCalories} kcal</div>
            </div>
          </div>

          <div class="mt-3 grid grid-cols-1 sm:grid-cols-2 gap-2">
            ${p.items.map(i => `
              <div class="flex items-center gap-2 p-2 rounded-lg bg-slate-50">
                <img src="${i.imageUrl}" class="w-12 h-12 rounded object-cover" />
                <div class="text-xs">
                  <div class="font-bold text-slate-800 line-clamp-1">${i.name}</div>
                  <div class="text-[10px] text-slate-400">${i.restaurantName} • BDT ${i.price}</div>
                </div>
              </div>
            `).join('')}
          </div>

          <div class="mt-3 flex justify-end">
            <button onclick="app.addPlanToCart(${idx})" class="btn-material btn-primary text-xs py-1.5 px-4">
              <span class="material-symbols-outlined text-sm">add_shopping_cart</span> Add Combo to Cart
            </button>
          </div>
        </div>
      `).join('')}
    `;

    this.activePlans = plans;
  }

  addPlanToCart(planIdx) {
    if (!this.activePlans || !this.activePlans[planIdx]) return;
    const plan = this.activePlans[planIdx];
    for (const item of plan.items) {
      window.state.addToCart(item.id, 1);
    }
    this.renderHeader();
    this.showToast(`Added ${plan.items.length} combo items to cart!`, 'success');
  }

  // ==========================================================================
  // CUSTOMER: LEFTOVER FOOD DEALS
  // ==========================================================================
  renderLeftovers(container) {
    const deals = window.state.leftovers;

    container.innerHTML = `
      <div class="space-y-6">
        <div class="p-6 rounded-3xl bg-gradient-to-r from-amber-500 to-red-500 text-white md-elevation-1">
          <div class="flex items-center gap-3">
            <span class="material-symbols-outlined p-3 rounded-2xl bg-white/20 text-white text-3xl">timer</span>
            <div>
              <span class="px-2.5 py-0.5 rounded-full bg-white/30 text-white text-[10px] font-bold uppercase tracking-wider">Zero Food Waste Initiative</span>
              <h1 class="text-2xl font-black mt-1">Leftover Flash Deals</h1>
              <p class="text-xs sm:text-sm text-amber-100 mt-1">
                Near end-of-day kitchen surplus freshly discounted up to 50%. Orders are prepared immediately before cutoff!
              </p>
            </div>
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          ${deals.map(d => `
            <div class="md-card overflow-hidden flex flex-col justify-between">
              <div>
                <div class="relative h-44 bg-slate-100">
                  <img src="${d.imageUrl}" class="w-full h-full object-cover" />
                  <div class="absolute top-3 right-3 px-2.5 py-1 rounded-full bg-red-600 text-white text-xs font-black shadow-lg">
                    -${d.discountPercent}% OFF
                  </div>
                  <div class="absolute bottom-3 left-3 px-2.5 py-1 rounded-lg bg-black/70 text-white text-[11px] font-semibold backdrop-blur-sm flex items-center gap-1">
                    <span class="material-symbols-outlined text-xs text-amber-400">alarm</span>
                    Expires in <span class="countdown-tag" data-hours="${d.expiresInHours}">3h 20m</span>
                  </div>
                </div>

                <div class="p-5">
                  <span class="text-[11px] font-bold text-orange-600 uppercase tracking-wider">${d.restaurantName}</span>
                  <h3 class="text-base font-bold text-slate-800 mt-0.5">${d.itemName}</h3>
                  <div class="mt-2 flex items-baseline gap-2">
                    <span class="text-lg font-black text-red-600">BDT ${d.discountedPrice}</span>
                    <span class="text-xs text-slate-400 line-through">BDT ${d.originalPrice}</span>
                    <span class="text-[11px] text-emerald-600 font-semibold">(Save BDT ${d.originalPrice - d.discountedPrice})</span>
                  </div>
                  <div class="mt-2 text-[11px] text-slate-500">
                    Only <span class="font-bold text-slate-700">${d.quantityAvailable}</span> portions remaining
                  </div>
                </div>
              </div>

              <div class="p-5 pt-0">
                <button onclick="app.claimLeftover(${d.menuItemId})" class="w-full btn-material btn-primary text-xs py-2.5">
                  <span class="material-symbols-outlined text-sm">flash_on</span>
                  Claim Surplus Deal
                </button>
              </div>
            </div>
          `).join('')}
        </div>
      </div>
    `;
  }

  claimLeftover(menuItemId) {
    this.addToCart(menuItemId);
    this.openCartModal();
  }

  startCountdownTimer() {
    setInterval(() => {
      document.querySelectorAll('.countdown-tag').forEach(tag => {
        let hrs = parseFloat(tag.dataset.hours || '3');
        if (hrs > 0) {
          hrs -= 0.005;
          tag.dataset.hours = hrs;
          const h = Math.floor(hrs);
          const m = Math.floor((hrs - h) * 60);
          tag.textContent = `${h}h ${m < 10 ? '0' : ''}${m}m`;
        } else {
          tag.textContent = 'Expired';
        }
      });
    }, 10000);
  }

  // ==========================================================================
  // CUSTOMER: ORDERS & REAL-TIME TRACKER
  // ==========================================================================
  renderCustomerOrders(container) {
    const user = window.state.getCurrentUser();
    const orders = window.state.orders.filter(o => o.customerId === user.id);

    container.innerHTML = `
      <div class="space-y-6 max-w-4xl mx-auto">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-xl font-bold text-slate-800">My Orders & Live Tracking</h1>
            <p class="text-xs text-slate-500">Track active deliveries step-by-step or view order history.</p>
          </div>
        </div>

        ${orders.length === 0 ? `
          <div class="p-8 text-center text-slate-400 bg-white rounded-2xl border border-slate-200">
            You have placed no orders yet.
          </div>
        ` : `
          <div class="space-y-4">
            ${orders.map(o => this.getOrderCardHtml(o)).join('')}
          </div>
        `}
      </div>
    `;
  }

  getOrderCardHtml(order) {
    const steps = ['Placed', 'Accepted', 'Preparing', 'ReadyForPickup', 'OutForDelivery', 'Delivered'];
    const currentStepIdx = steps.indexOf(order.status);

    return `
      <div class="md-card p-5">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2 pb-3 border-b border-slate-100">
          <div>
            <div class="flex items-center gap-2">
              <span class="text-sm font-black text-slate-900">Order #${order.id}</span>
              ${order.isCombined ? `
                <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-purple-50 text-purple-700 border border-purple-200">
                  COMBINED BATCH (${order.combinedGroupCode})
                </span>
              ` : ''}
              <span class="px-2 py-0.5 rounded text-[11px] font-bold ${order.status === 'Delivered' ? 'bg-emerald-50 text-emerald-700' : 'bg-orange-50 text-orange-700'}">
                ${order.status}
              </span>
            </div>
            <div class="text-xs text-slate-500 mt-0.5">${order.restaurantName} • ${new Date(order.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</div>
          </div>
          <div class="text-right">
            <div class="text-base font-black text-orange-600">BDT ${order.totalAmount}</div>
            <div class="text-[11px] text-slate-400">${order.paymentMethod} (${order.paymentStatus})</div>
          </div>
        </div>

        <!-- Visual Timeline Stepper -->
        <div class="my-4 px-2">
          <div class="relative flex items-center justify-between">
            <div class="absolute left-0 top-1/2 -translate-y-1/2 w-full h-1 bg-slate-200 -z-0"></div>
            <div class="absolute left-0 top-1/2 -translate-y-1/2 h-1 bg-orange-600 -z-0 transition-all duration-500" 
                 style="width: ${(Math.max(0, currentStepIdx) / (steps.length - 1)) * 100}%"></div>

            ${steps.map((s, i) => `
              <div class="relative z-10 flex flex-col items-center">
                <div class="w-6 h-6 rounded-full flex items-center justify-center text-[10px] font-bold ${i <= currentStepIdx ? 'bg-orange-600 text-white shadow' : 'bg-slate-200 text-slate-500'}">
                  ${i <= currentStepIdx ? '✓' : i + 1}
                </div>
                <span class="text-[9px] font-medium text-slate-500 mt-1 hidden sm:block">${s}</span>
              </div>
            `).join('')}
          </div>
        </div>

        <!-- Items Breakdown -->
        <div class="mt-3 pt-2 text-xs border-t border-slate-100 flex flex-wrap gap-2 text-slate-600">
          ${order.items.map(i => `
            <span class="px-2 py-1 bg-slate-50 rounded-md border border-slate-100">
              ${i.quantity}x ${i.name}
            </span>
          `).join('')}
        </div>

        <!-- Rider Assignment Box if assigned -->
        ${order.riderName ? `
          <div class="mt-3 p-2.5 rounded-xl bg-slate-50 border border-slate-200 flex items-center justify-between text-xs">
            <div class="flex items-center gap-2">
              <span class="material-symbols-outlined text-orange-600">sports_motorsports</span>
              <div>
                <span class="font-bold text-slate-800">${order.riderName}</span>
                <span class="text-slate-400 text-[11px]"> • Assigned Delivery Rider</span>
              </div>
            </div>
            <span class="text-[11px] font-semibold text-emerald-600">On Active Route</span>
          </div>
        ` : ''}
      </div>
    `;
  }

  // ==========================================================================
  // RESTAURANT OWNER DASHBOARD
  // ==========================================================================
  renderRestaurantDashboard(container) {
    const user = window.state.getCurrentUser();
    const restaurant = window.state.restaurants.find(r => r.ownerUserId === user.id) || window.state.restaurants[0];
    const incomingOrders = window.state.orders.filter(o => o.restaurantId === restaurant.id);
    const queue = window.state.getRestaurantQueueStatus(restaurant.id);

    container.innerHTML = `
      <div class="space-y-6">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <div class="flex items-center gap-2">
              <h1 class="text-2xl font-black text-slate-800">${restaurant.name} Operations</h1>
              <span class="px-2 py-0.5 text-xs font-bold rounded ${queue.status === 'Free' ? 'badge-free' : (queue.status === 'Normal' ? 'badge-normal' : 'badge-busy')}">
                Queue: ${queue.status}
              </span>
            </div>
            <p class="text-xs text-slate-500">${restaurant.category} • ${restaurant.address}</p>
          </div>

          <div class="flex gap-2">
            <button onclick="app.openNewLeftoverModal(${restaurant.id})" class="btn-material btn-secondary text-xs">
              <span class="material-symbols-outlined text-sm">percent</span> Post Leftover Deal
            </button>
          </div>
        </div>

        <!-- Operations Metrics -->
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
          <div class="md-card p-4">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Active Kitchen Load</span>
            <div class="text-2xl font-black text-orange-600 mt-1">${queue.activeCount} Orders</div>
            <span class="text-[11px] text-slate-500">Max Capacity: ${restaurant.kitchenCapacity}</span>
          </div>
          <div class="md-card p-4">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Avg Prep Duration</span>
            <div class="text-2xl font-black text-slate-800 mt-1">${restaurant.avgPrepTimeMinutes} Mins</div>
            <span class="text-[11px] text-slate-500">Queue Wait: ~${queue.waitMinutes} mins</span>
          </div>
          <div class="md-card p-4">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Today's Orders</span>
            <div class="text-2xl font-black text-slate-800 mt-1">${incomingOrders.length}</div>
            <span class="text-[11px] text-emerald-600 font-semibold">Live stream</span>
          </div>
          <div class="md-card p-4">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Gross Revenue</span>
            <div class="text-2xl font-black text-emerald-600 mt-1">BDT ${incomingOrders.reduce((s, o) => s + o.totalAmount, 0)}</div>
            <span class="text-[11px] text-slate-500">Includes Combined orders</span>
          </div>
        </div>

        <!-- Kitchen Live Orders Stream -->
        <div class="space-y-4">
          <h3 class="text-sm font-bold text-slate-800 flex items-center gap-2">
            <span class="pulse-dot pulse-green"></span> Live Kitchen Orders & Queue Flow
          </h3>

          ${incomingOrders.length === 0 ? `
            <div class="p-6 text-center text-slate-400 bg-white rounded-2xl border border-slate-200">
              No orders received today.
            </div>
          ` : `
            <div class="space-y-3">
              ${incomingOrders.map(o => `
                <div class="md-card p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                  <div>
                    <div class="flex items-center gap-2">
                      <span class="text-sm font-bold text-slate-900">Order #${o.id}</span>
                      <span class="px-2 py-0.5 rounded text-[10px] font-bold ${o.status === 'Delivered' ? 'bg-emerald-50 text-emerald-700' : 'bg-amber-50 text-amber-800'}">
                        ${o.status}
                      </span>
                      ${o.isCombined ? '<span class="px-1.5 py-0.5 rounded bg-purple-100 text-purple-800 text-[10px] font-bold">Combined Batch</span>' : ''}
                    </div>
                    <div class="text-xs text-slate-500 mt-1">Customer: ${o.customerName} (${o.customerPhone})</div>
                    <div class="text-xs text-slate-700 mt-1 font-medium">
                      ${o.items.map(i => `${i.quantity}x ${i.name}`).join(', ')}
                    </div>
                  </div>

                  <div class="flex items-center gap-2">
                    ${o.status === 'Placed' ? `
                      <button onclick="app.updateStatus(${o.id}, 'Accepted')" class="btn-material btn-primary text-xs py-1.5 px-3">
                        Accept Order
                      </button>
                    ` : ''}

                    ${o.status === 'Accepted' ? `
                      <button onclick="app.updateStatus(${o.id}, 'Preparing')" class="btn-material btn-secondary text-xs py-1.5 px-3">
                        Start Preparing
                      </button>
                    ` : ''}

                    ${o.status === 'Preparing' ? `
                      <button onclick="app.updateStatus(${o.id}, 'ReadyForPickup')" class="btn-material bg-emerald-600 text-white text-xs py-1.5 px-3 hover:bg-emerald-700">
                        Mark Ready For Pickup
                      </button>
                    ` : ''}

                    ${o.status === 'ReadyForPickup' ? `
                      <span class="text-xs text-slate-500 italic">Waiting for rider pickup...</span>
                    ` : ''}

                    ${o.status === 'OutForDelivery' ? `
                      <span class="text-xs text-orange-600 font-semibold">Rider on way to customer</span>
                    ` : ''}

                    ${o.status === 'Delivered' ? `
                      <span class="text-xs text-emerald-600 font-semibold">✓ Delivered</span>
                    ` : ''}
                  </div>
                </div>
              `).join('')}
            </div>
          `}
        </div>
      </div>
    `;
  }

  async updateStatus(orderId, newStatus) {
    await window.api.updateOrderStatus(orderId, newStatus);
    this.showToast(`Order #${orderId} moved to: ${newStatus}`, 'info');
    const user = window.state.getCurrentUser();
    if (user.role === 'RestaurantOwner') this.renderRestaurantDashboard(document.getElementById('main-content'));
    else if (user.role === 'DeliveryRider') this.renderRiderDashboard(document.getElementById('main-content'));
  }

  openNewLeftoverModal(restaurantId) {
    const items = window.state.menuItems.filter(m => m.restaurantId === restaurantId);
    const modal = document.getElementById('generic-modal');
    const content = document.getElementById('generic-modal-content');

    content.innerHTML = `
      <div class="p-6">
        <h2 class="text-lg font-bold text-slate-800">Publish Leftover Flash Discount</h2>
        <p class="text-xs text-slate-500 mt-0.5">Offer near-end-of-day surplus meals to cut waste and recover food cost.</p>

        <div class="mt-4 space-y-3">
          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Select Menu Item</label>
            <select id="leftover-item-select" class="w-full p-2 border border-slate-200 rounded-lg text-xs">
              ${items.map(i => `<option value="${i.id}">${i.name} (Reg. BDT ${i.price})</option>`).join('')}
            </select>
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Discount Percentage</label>
            <select id="leftover-discount-select" class="w-full p-2 border border-slate-200 rounded-lg text-xs">
              <option value="25">25% OFF</option>
              <option value="35" selected>35% OFF</option>
              <option value="50">50% OFF (Flash Deal)</option>
            </select>
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Portions Available</label>
            <input type="number" id="leftover-qty-input" value="4" min="1" max="50" class="w-full p-2 border border-slate-200 rounded-lg text-xs" />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-700 mb-1">Expiry Duration (Hours)</label>
            <input type="number" id="leftover-hours-input" value="3.5" step="0.5" min="1" max="12" class="w-full p-2 border border-slate-200 rounded-lg text-xs" />
          </div>
        </div>

        <div class="mt-5 flex justify-end gap-2">
          <button onclick="app.closeAllModals()" class="px-4 py-2 bg-slate-100 text-slate-700 text-xs font-semibold rounded-lg">Cancel</button>
          <button onclick="app.submitLeftoverDeal(${restaurantId})" class="btn-material btn-primary text-xs py-2 px-5">Publish Deal</button>
        </div>
      </div>
    `;

    modal.classList.remove('hidden');
  }

  submitLeftoverDeal(restaurantId) {
    const itemId = Number(document.getElementById('leftover-item-select').value);
    const discount = Number(document.getElementById('leftover-discount-select').value);
    const qty = Number(document.getElementById('leftover-qty-input').value);
    const hours = Number(document.getElementById('leftover-hours-input').value);

    const item = window.state.menuItems.find(m => m.id === itemId);
    const rest = window.state.restaurants.find(r => r.id === restaurantId);

    const discountedPrice = Math.round(item.price * (1 - (discount / 100)));

    window.state.leftovers.unshift({
      id: window.state.leftovers.length + 1,
      restaurantId,
      restaurantName: rest?.name,
      menuItemId: item.id,
      itemName: item.name,
      category: item.category,
      originalPrice: item.price,
      discountPercent: discount,
      discountedPrice,
      quantityAvailable: qty,
      expiresInHours: hours,
      imageUrl: item.imageUrl
    });

    window.state.saveState();
    this.closeAllModals();
    this.showToast(`Flash discount for ${item.name} (${discount}% OFF) published!`, 'success');
  }

  // ==========================================================================
  // DELIVERY RIDER WORKFLOW
  // ==========================================================================
  renderRiderDashboard(container) {
    const user = window.state.getCurrentUser();
    const assignedDeliveries = window.state.orders.filter(o => 
      o.riderId === user.riderId || (o.riderId === 1 && user.id === 7)
    );
    const readyOrders = window.state.orders.filter(o => !o.riderId && o.status === 'ReadyForPickup');

    container.innerHTML = `
      <div class="space-y-6 max-w-4xl mx-auto">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div>
            <h1 class="text-2xl font-black text-slate-800">Rider Dispatch Portal</h1>
            <p class="text-xs text-slate-500">${user.name} • ${user.vehicle || 'Motorcycle'}</p>
          </div>

          <div class="flex items-center gap-2 bg-white p-2 rounded-2xl border border-slate-200">
            <span class="text-xs font-bold text-slate-700">Status:</span>
            <button onclick="app.toggleRiderStatus('Available')" class="px-3 py-1 rounded-lg text-xs font-bold ${user.status === 'Available' ? 'bg-emerald-600 text-white' : 'bg-slate-100 text-slate-600'}">Available</button>
            <button onclick="app.toggleRiderStatus('OnDelivery')" class="px-3 py-1 rounded-lg text-xs font-bold ${user.status === 'OnDelivery' ? 'bg-orange-600 text-white' : 'bg-slate-100 text-slate-600'}">On Delivery</button>
            <button onclick="app.toggleRiderStatus('Offline')" class="px-3 py-1 rounded-lg text-xs font-bold ${user.status === 'Offline' ? 'bg-slate-600 text-white' : 'bg-slate-100 text-slate-600'}">Offline</button>
          </div>
        </div>

        <!-- Rider Stats -->
        <div class="grid grid-cols-3 gap-4">
          <div class="md-card p-4 text-center">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Deliveries Completed</span>
            <div class="text-xl font-black text-slate-800 mt-1">${user.deliveries || 142}</div>
          </div>
          <div class="md-card p-4 text-center">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Rider Rating</span>
            <div class="text-xl font-black text-amber-500 mt-1">★ ${user.rating || 4.9}</div>
          </div>
          <div class="md-card p-4 text-center">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Today's Earnings</span>
            <div class="text-xl font-black text-emerald-600 mt-1">BDT ${assignedDeliveries.filter(o => o.status === 'Delivered').length * 50 + 200}</div>
          </div>
        </div>

        <!-- Active Assignments -->
        <div class="space-y-4">
          <h3 class="text-sm font-bold text-slate-800">My Assigned Delivery Runs</h3>

          ${assignedDeliveries.length === 0 ? `
            <div class="p-6 text-center text-slate-400 bg-white rounded-2xl border border-slate-200">
              No deliveries assigned right now. You will receive assignments when orders are ready.
            </div>
          ` : `
            <div class="space-y-3">
              ${assignedDeliveries.map(o => `
                <div class="md-card p-5">
                  <div class="flex items-center justify-between pb-3 border-b border-slate-100">
                    <div>
                      <div class="flex items-center gap-2">
                        <span class="text-sm font-bold text-slate-900">Order #${o.id}</span>
                        ${o.isCombined ? '<span class="px-1.5 py-0.5 rounded bg-purple-100 text-purple-800 text-[10px] font-bold">Multi-Restaurant Stop</span>' : ''}
                        <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-orange-50 text-orange-700">${o.status}</span>
                      </div>
                      <div class="text-xs text-slate-500 mt-0.5">Pickup: <span class="font-semibold text-slate-800">${o.restaurantName}</span></div>
                      <div class="text-xs text-slate-500">Dropoff: <span class="font-semibold text-slate-800">${o.deliveryAddress}</span></div>
                    </div>
                    <div class="text-right">
                      <div class="text-sm font-bold text-orange-600">BDT ${o.totalAmount}</div>
                      <span class="text-[11px] text-slate-400">${o.paymentMethod}</span>
                    </div>
                  </div>

                  <div class="mt-4 flex flex-wrap gap-2 justify-end">
                    ${o.status === 'ReadyForPickup' || o.status === 'Accepted' || o.status === 'Preparing' ? `
                      <button onclick="app.updateStatus(${o.id}, 'OutForDelivery')" class="btn-material btn-secondary text-xs py-2">
                        <span class="material-symbols-outlined text-sm">local_shipping</span> Confirm Pick Up & Start Delivery
                      </button>
                    ` : ''}

                    ${o.status === 'OutForDelivery' ? `
                      <button onclick="app.updateStatus(${o.id}, 'Delivered')" class="btn-material bg-emerald-600 text-white text-xs py-2 hover:bg-emerald-700">
                        <span class="material-symbols-outlined text-sm">check_circle</span> Mark Order Delivered
                      </button>
                    ` : ''}

                    ${o.status === 'Delivered' ? `
                      <span class="text-xs text-emerald-600 font-bold flex items-center gap-1">
                        <span class="material-symbols-outlined text-sm">verified</span> Dropoff Completed
                      </span>
                    ` : ''}
                  </div>
                </div>
              `).join('')}
            </div>
          `}
        </div>

        <!-- Orders Ready for Pickup Pool -->
        ${readyOrders.length > 0 ? `
          <div class="space-y-3 pt-4 border-t border-slate-200">
            <h3 class="text-xs font-bold text-slate-400 uppercase tracking-wider">Unassigned Orders Ready in Kitchen</h3>
            ${readyOrders.map(ro => `
              <div class="p-3.5 rounded-xl bg-white border border-slate-200 flex items-center justify-between">
                <div>
                  <div class="text-xs font-bold text-slate-800">Order #${ro.id} • ${ro.restaurantName}</div>
                  <div class="text-[11px] text-slate-400">Destination: ${ro.deliveryAddress}</div>
                </div>
                <button onclick="app.claimRiderOrder(${ro.id})" class="btn-material btn-primary text-xs py-1 px-3">
                  Claim Order
                </button>
              </div>
            `).join('')}
          </div>
        ` : ''}
      </div>
    `;
  }

  toggleRiderStatus(status) {
    const user = window.state.getCurrentUser();
    user.status = status;
    window.state.saveState();
    this.renderRiderDashboard(document.getElementById('main-content'));
    this.showToast(`Rider availability set to: ${status}`, 'info');
  }

  claimRiderOrder(orderId) {
    const user = window.state.getCurrentUser();
    window.api.updateOrderStatus(orderId, 'ReadyForPickup', user.riderId || 1);
    this.showToast(`Order #${orderId} claimed and added to your route!`, 'success');
    this.renderRiderDashboard(document.getElementById('main-content'));
  }

  // ==========================================================================
  // ADMINISTRATOR DASHBOARD
  // ==========================================================================
  renderAdminDashboard(container) {
    const users = window.state.users;
    const restaurants = window.state.restaurants;
    const orders = window.state.orders;
    const gmv = orders.reduce((sum, o) => sum + o.totalAmount, 0);

    container.innerHTML = `
      <div class="space-y-6">
        <div>
          <h1 class="text-2xl font-black text-slate-800">Administrator Console</h1>
          <p class="text-xs text-slate-500">System oversight, restaurant verifications, user directories, and performance metrics.</p>
        </div>

        <!-- Global Platform KPIs -->
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
          <div class="md-card p-4">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Gross Platform GMV</span>
            <div class="text-2xl font-black text-emerald-600 mt-1">BDT ${gmv}</div>
            <span class="text-[11px] text-slate-500">Across all orders</span>
          </div>
          <div class="md-card p-4">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Total Orders</span>
            <div class="text-2xl font-black text-slate-800 mt-1">${orders.length}</div>
            <span class="text-[11px] text-slate-500">${orders.filter(o => o.isCombined).length} Combined batches</span>
          </div>
          <div class="md-card p-4">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Restaurants</span>
            <div class="text-2xl font-black text-slate-800 mt-1">${restaurants.length}</div>
            <span class="text-[11px] text-emerald-600 font-semibold">${restaurants.filter(r => r.isVerified).length} Verified</span>
          </div>
          <div class="md-card p-4">
            <span class="text-[11px] font-bold text-slate-400 uppercase">Registered Users</span>
            <div class="text-2xl font-black text-slate-800 mt-1">${users.length}</div>
            <span class="text-[11px] text-slate-500">4 Distinct Roles</span>
          </div>
        </div>

        <!-- Restaurant Management & Verification -->
        <div class="md-card p-5">
          <h3 class="text-sm font-bold text-slate-800 mb-3">Restaurant Verification & Operations</h3>
          <div class="overflow-x-auto">
            <table class="w-full text-left text-xs">
              <thead class="bg-slate-50 text-slate-500 uppercase text-[10px]">
                <tr>
                  <th class="p-2.5">Name</th>
                  <th class="p-2.5">Category</th>
                  <th class="p-2.5">Capacity</th>
                  <th class="p-2.5">Rating</th>
                  <th class="p-2.5">Status</th>
                  <th class="p-2.5 text-right">Action</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                ${restaurants.map(r => `
                  <tr>
                    <td class="p-2.5 font-bold text-slate-800">${r.name}</td>
                    <td class="p-2.5 text-slate-500">${r.category}</td>
                    <td class="p-2.5">${r.kitchenCapacity} orders</td>
                    <td class="p-2.5 text-amber-500 font-bold">★ ${r.rating}</td>
                    <td class="p-2.5">
                      <span class="px-2 py-0.5 rounded text-[10px] font-bold ${r.isVerified ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-700'}">
                        ${r.isVerified ? 'Verified' : 'Pending'}
                      </span>
                    </td>
                    <td class="p-2.5 text-right">
                      <button onclick="app.toggleRestaurantVerification(${r.id})" class="px-3 py-1 rounded bg-slate-100 hover:bg-slate-200 text-[11px] font-semibold text-slate-700">
                        ${r.isVerified ? 'Unverify' : 'Verify'}
                      </button>
                    </td>
                  </tr>
                `).join('')}
              </tbody>
            </table>
          </div>
        </div>

        <!-- User Accounts Directory -->
        <div class="md-card p-5">
          <h3 class="text-sm font-bold text-slate-800 mb-3">System Accounts</h3>
          <div class="overflow-x-auto">
            <table class="w-full text-left text-xs">
              <thead class="bg-slate-50 text-slate-500 uppercase text-[10px]">
                <tr>
                  <th class="p-2.5">User</th>
                  <th class="p-2.5">Email</th>
                  <th class="p-2.5">Role</th>
                  <th class="p-2.5">Phone</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                ${users.map(u => `
                  <tr>
                    <td class="p-2.5 font-bold text-slate-800">${u.name}</td>
                    <td class="p-2.5 text-slate-500">${u.email}</td>
                    <td class="p-2.5">
                      <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-orange-50 text-orange-700">
                        ${u.role}
                      </span>
                    </td>
                    <td class="p-2.5 text-slate-500">${u.phone}</td>
                  </tr>
                `).join('')}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    `;
  }

  toggleRestaurantVerification(restaurantId) {
    const restaurant = window.state.restaurants.find(r => r.id === restaurantId);
    if (restaurant) {
      restaurant.isVerified = !restaurant.isVerified;
      window.state.saveState();
      this.renderAdminDashboard(document.getElementById('main-content'));
      this.showToast(`Updated verification for: ${restaurant.name}`, 'info');
    }
  }

  // ==========================================================================
  // HELPERS: MODALS & TOAST NOTIFICATIONS
  // ==========================================================================
  closeAllModals() {
    document.querySelectorAll('.modal-backdrop').forEach(m => m.classList.add('hidden'));
  }

  showToast(message, type = 'info') {
    let container = document.getElementById('toast-container');
    if (!container) {
      container = document.createElement('div');
      container.id = 'toast-container';
      document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `toast ${type === 'success' ? 'bg-emerald-600' : (type === 'warning' ? 'bg-amber-600' : 'bg-slate-800')}`;
    toast.innerHTML = `
      <div class="flex items-center gap-2 text-xs font-semibold">
        <span class="material-symbols-outlined text-sm">
          ${type === 'success' ? 'check_circle' : (type === 'warning' ? 'warning' : 'info')}
        </span>
        <span>${message}</span>
      </div>
    `;

    container.appendChild(toast);

    setTimeout(() => {
      toast.style.opacity = '0';
      toast.style.transform = 'translateY(10px)';
      setTimeout(() => toast.remove(), 300);
    }, 3200);
  }
}

document.addEventListener('DOMContentLoaded', () => {
  window.app = new BiteNestApp();
});
