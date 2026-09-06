// ============================================================================
// BiteNest — Unified API Service with Live / Offline Dual-Mode Adapter
// ============================================================================

const API_BASE_URL = 'http://localhost:5000/api';

class ApiService {
  constructor() {
    this.token = localStorage.getItem('bitenest_token') || null;
    this.isBackendAvailable = false;
    this.checkHealth();
  }

  async checkHealth() {
    try {
      const res = await fetch('http://localhost:5000/', { method: 'GET', signal: AbortSignal.timeout(1500) });
      if (res.ok) {
        this.isBackendAvailable = true;
        console.log('✅ BiteNest ASP.NET Core API Connected.');
      }
    } catch (e) {
      this.isBackendAvailable = false;
      console.info('ℹ️ BiteNest operating in high-performance local interactive mode.');
    }
  }

  setToken(token) {
    this.token = token;
    if (token) {
      localStorage.setItem('bitenest_token', token);
    } else {
      localStorage.removeItem('bitenest_token');
    }
  }

  async request(endpoint, options = {}) {
    if (!this.isBackendAvailable) {
      return null; // Signals caller to use local state adapter
    }

    try {
      const headers = {
        'Content-Type': 'application/json',
        ...(this.token ? { 'Authorization': `Bearer ${this.token}` } : {}),
        ...options.headers
      };

      const res = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers,
        signal: AbortSignal.timeout(3500)
      });

      if (!res.ok) {
        throw new Error(`HTTP ${res.status}: ${await res.text()}`);
      }

      return await res.json();
    } catch (err) {
      console.warn(`API request to ${endpoint} failed, falling back to client state.`, err);
      return null;
    }
  }

  // --- Auth APIs ---
  async login(email, password) {
    const remote = await this.request('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });
    if (remote) {
      this.setToken(remote.token);
      return remote;
    }

    // Local fallback
    const user = window.state.users.find(u => u.email.toLowerCase() === email.toLowerCase());
    if (user) {
      window.state.setCurrentUser(user.id);
      return { ...user, token: 'mock-jwt-token' };
    }
    throw new Error('Invalid email or password.');
  }

  async register(data) {
    const remote = await this.request('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data)
    });
    if (remote) return remote;

    // Local fallback
    const newUser = {
      id: window.state.users.length + 1,
      ...data,
      isVerified: true
    };
    window.state.users.push(newUser);
    window.state.setCurrentUser(newUser.id);
    window.state.saveState();
    return newUser;
  }

  // --- Restaurants APIs ---
  async getRestaurants(category = 'All', search = '') {
    const query = new URLSearchParams();
    if (category && category !== 'All') query.append('category', category);
    if (search) query.append('search', search);

    const remote = await this.request(`/restaurants?${query.toString()}`);
    if (remote) return remote;

    // Local fallback with real-time queue calculation
    return window.state.restaurants
      .filter(r => {
        const matchesCat = category === 'All' || r.category.includes(category);
        const matchesSearch = !search || r.name.toLowerCase().includes(search.toLowerCase()) || (r.description && r.description.toLowerCase().includes(search.toLowerCase()));
        return matchesCat && matchesSearch;
      })
      .map(r => {
        const queue = window.state.getRestaurantQueueStatus(r.id);
        return {
          ...r,
          queueStatus: queue.status,
          activeOrdersCount: queue.activeCount,
          estimatedWaitTimeMinutes: queue.waitMinutes
        };
      });
  }

  // --- Orders APIs ---
  async createOrder(orderData) {
    const remote = await this.request('/orders', {
      method: 'POST',
      body: JSON.stringify(orderData)
    });
    if (remote) return remote;

    // Local order creation
    const user = window.state.getCurrentUser();
    const newOrder = {
      id: window.state.orders.length + 101,
      customerId: user.id,
      customerName: user.name,
      customerPhone: user.phone,
      deliveryAddress: orderData.deliveryAddress,
      restaurantId: orderData.restaurantId,
      restaurantName: window.state.restaurants.find(r => r.id === orderData.restaurantId)?.name || 'Restaurant',
      isCombined: false,
      combinedGroupCode: null,
      status: 'Placed',
      totalAmount: orderData.totalAmount || 420,
      deliveryFee: 40,
      discountAmount: 0,
      paymentMethod: orderData.paymentMethod || 'CashOnDelivery',
      paymentStatus: orderData.paymentMethod === 'CashOnDelivery' ? 'Pending' : 'Completed',
      riderId: null,
      preferredDeliveryTime: orderData.preferredDeliveryTime,
      createdAt: new Date().toISOString(),
      items: orderData.items || []
    };

    window.state.orders.unshift(newOrder);
    window.state.clearCart();
    window.state.saveState();
    return newOrder;
  }

  async createCombinedOrder(combinedData) {
    const remote = await this.request('/orders/combined', {
      method: 'POST',
      body: JSON.stringify(combinedData)
    });
    if (remote) return remote;

    const user = window.state.getCurrentUser();
    const groupCode = `COMB-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(1000 + Math.random() * 9000)}`;

    const r1 = window.state.restaurants.find(r => r.id === combinedData.restaurantId1);
    const r2 = window.state.restaurants.find(r => r.id === combinedData.restaurantId2);

    const order1 = {
      id: window.state.orders.length + 101,
      customerId: user.id,
      customerName: user.name,
      customerPhone: user.phone,
      deliveryAddress: combinedData.deliveryAddress,
      restaurantId: combinedData.restaurantId1,
      restaurantName: r1?.name || 'Restaurant 1',
      isCombined: true,
      combinedGroupCode: groupCode,
      status: 'Placed',
      totalAmount: combinedData.total1 + 30,
      deliveryFee: 30,
      discountAmount: 10,
      paymentMethod: combinedData.paymentMethod || 'bKash',
      paymentStatus: 'Completed',
      riderId: null,
      preferredDeliveryTime: combinedData.preferredDeliveryTime,
      createdAt: new Date().toISOString(),
      items: combinedData.items1
    };

    const order2 = {
      id: window.state.orders.length + 102,
      customerId: user.id,
      customerName: user.name,
      customerPhone: user.phone,
      deliveryAddress: combinedData.deliveryAddress,
      restaurantId: combinedData.restaurantId2,
      restaurantName: r2?.name || 'Restaurant 2',
      isCombined: true,
      combinedGroupCode: groupCode,
      status: 'Placed',
      totalAmount: combinedData.total2 + 30,
      deliveryFee: 30,
      discountAmount: 10,
      paymentMethod: combinedData.paymentMethod || 'bKash',
      paymentStatus: 'Completed',
      riderId: null,
      preferredDeliveryTime: combinedData.preferredDeliveryTime,
      createdAt: new Date().toISOString(),
      items: combinedData.items2
    };

    window.state.orders.unshift(order1, order2);
    window.state.clearCart();
    window.state.saveState();
    return { order1, order2, groupCode, savings: 20 };
  }

  async updateOrderStatus(orderId, newStatus, riderId = null) {
    const remote = await this.request(`/orders/${orderId}/status`, {
      method: 'PUT',
      body: JSON.stringify({ newStatus, riderId })
    });
    if (remote) return remote;

    const order = window.state.orders.find(o => o.id === orderId);
    if (order) {
      order.status = newStatus;
      if (riderId) order.riderId = riderId;

      // Sibling update if combined
      if (order.isCombined && order.combinedGroupCode) {
        const siblings = window.state.orders.filter(o => o.combinedGroupCode === order.combinedGroupCode && o.id !== order.id);
        siblings.forEach(s => {
          if (riderId) s.riderId = riderId;
          if (newStatus === 'OutForDelivery' || newStatus === 'Delivered') {
            s.status = newStatus;
          }
        });
      }
      window.state.saveState();
    }
    return order;
  }
}

window.api = new ApiService();
