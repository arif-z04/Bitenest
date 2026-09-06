// ============================================================================
// BiteNest — Central State Management & Default Offline/Online Mock Store
// ============================================================================

const DEFAULT_USERS = [
  { id: 1, name: 'System Administrator', email: 'admin@bitenest.com', role: 'Administrator', phone: '+8801711000001', address: 'BiteNest HQ, Dhanmondi, Dhaka' },
  { id: 2, name: 'SpiceCraft Manager', email: 'owner.spicecraft@bitenest.com', role: 'RestaurantOwner', phone: '+8801711000002', address: 'House 42, Road 7A, Dhanmondi, Dhaka', restaurantId: 1 },
  { id: 3, name: 'Burger Barn Owner', email: 'owner.burgerbarn@bitenest.com', role: 'RestaurantOwner', phone: '+8801711000003', address: 'Plot 15, Satmasjid Road, Dhanmondi, Dhaka', restaurantId: 2 },
  { id: 4, name: 'Green Bowls Director', email: 'owner.greenbowls@bitenest.com', role: 'RestaurantOwner', phone: '+8801711000004', address: 'Block C, Road 27, Dhanmondi, Dhaka', restaurantId: 3 },
  { id: 5, name: 'Rahim Ahmed', email: 'customer.rahim@bitenest.com', role: 'Customer', phone: '+8801711000005', address: 'Apartment 4B, Road 12, Dhanmondi, Dhaka', lat: 23.7525, lng: 90.3765 },
  { id: 6, name: 'Fatima Jahan', email: 'customer.fatima@bitenest.com', role: 'Customer', phone: '+8801711000006', address: 'House 88, Road 8, Dhanmondi, Dhaka', lat: 23.7495, lng: 90.3740 },
  { id: 7, name: 'Tanvir Hasan (Rider 1)', email: 'rider.tanvir@bitenest.com', role: 'DeliveryRider', phone: '+8801711000007', address: 'Shankar Stand, Dhanmondi', vehicle: 'Motorcycle', status: 'Available', rating: 4.9, deliveries: 142, riderId: 1 },
  { id: 8, name: 'Sumon Barua (Rider 2)', email: 'rider.sumon@bitenest.com', role: 'DeliveryRider', phone: '+8801711000008', address: 'Zigatola Bus Stand, Dhanmondi', vehicle: 'Motorcycle', status: 'OnDelivery', rating: 4.8, deliveries: 89, riderId: 2 }
];

const DEFAULT_RESTAURANTS = [
  {
    id: 1,
    ownerUserId: 2,
    name: 'SpiceCraft Kitchen',
    description: 'Authentic slow-cooked biryani, aromatic curries, and freshly baked tandoori naans.',
    category: 'Bengali & Indian',
    address: 'House 42, Road 7A, Dhanmondi, Dhaka',
    lat: 23.7508,
    lng: 90.3752,
    phone: '+8801711000002',
    imageUrl: 'https://images.unsplash.com/photo-1589302168068-964664d93dc0?w=600&auto=format&fit=crop&q=80',
    isVerified: true,
    isOpen: true,
    avgPrepTimeMinutes: 25,
    kitchenCapacity: 12,
    rating: 4.8
  },
  {
    id: 2,
    ownerUserId: 3,
    name: 'Burger Barn & Grill',
    description: 'Gourmet smashed beef burgers, crispy buttermilk chicken tenders, and loaded cheese fries.',
    category: 'Fast Food & Burgers',
    address: 'Plot 15, Satmasjid Road, Dhanmondi, Dhaka',
    lat: 23.7540,
    lng: 90.3780,
    phone: '+8801711000003',
    imageUrl: 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80',
    isVerified: true,
    isOpen: true,
    avgPrepTimeMinutes: 15,
    kitchenCapacity: 18,
    rating: 4.6
  },
  {
    id: 3,
    ownerUserId: 4,
    name: 'Green Bowls & Juices',
    description: 'Nutrient-rich protein grain bowls, detox fresh cold-pressed juices, and organic wraps.',
    category: 'Healthy & Salads',
    address: 'Block C, Road 27, Dhanmondi, Dhaka',
    lat: 23.7580,
    lng: 90.3810,
    phone: '+8801711000004',
    imageUrl: 'https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&auto=format&fit=crop&q=80',
    isVerified: true,
    isOpen: true,
    avgPrepTimeMinutes: 12,
    kitchenCapacity: 10,
    rating: 4.7
  }
];

const DEFAULT_MENU_ITEMS = [
  { id: 1, restaurantId: 1, name: 'Kacchi Biryani Special', description: 'Fragrant basmati rice layered with tender mutton chunks and spiced saffron potato.', price: 380, category: 'Main Course', prepTimeMinutes: 25, calories: 820, imageUrl: 'https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=600&auto=format&fit=crop&q=80', isAvailable: true },
  { id: 2, restaurantId: 1, name: 'Butter Chicken Masala', description: 'Charcoal-grilled chicken simmered in velvety tomato, cashew, and cream gravy.', price: 320, category: 'Main Course', prepTimeMinutes: 20, calories: 580, imageUrl: 'https://images.unsplash.com/photo-1603894584373-5ac82b2ae398?w=600&auto=format&fit=crop&q=80', isAvailable: true },
  { id: 3, restaurantId: 1, name: 'Garlic Butter Naan', description: 'Tandoor baked flatbread brushed with roasted garlic butter and cilantro.', price: 60, category: 'Bread', prepTimeMinutes: 10, calories: 210, imageUrl: 'https://images.unsplash.com/photo-1601050690597-df0568f70950?w=600&auto=format&fit=crop&q=80', isAvailable: true },
  { id: 4, restaurantId: 1, name: 'Borhani Pitcher (500ml)', description: 'Traditional spiced yogurt digestive drink with mint, coriander, and black rock salt.', price: 90, category: 'Beverages', prepTimeMinutes: 5, calories: 140, imageUrl: 'https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=600&auto=format&fit=crop&q=80', isAvailable: true },

  { id: 7, restaurantId: 2, name: 'Classic Smokehouse Beef Burger', description: '150g grilled beef patty, melted cheddar, caramelized onions, and house smoky BBQ mayo.', price: 290, category: 'Burgers', prepTimeMinutes: 15, calories: 690, imageUrl: 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80', isAvailable: true },
  { id: 8, restaurantId: 2, name: 'Crispy Hot Buttermilk Chicken Burger', description: 'Double fried crunchy chicken thigh tossed in spicy cayenne butter with crisp dill pickles.', price: 270, category: 'Burgers', prepTimeMinutes: 15, calories: 640, imageUrl: 'https://images.unsplash.com/photo-1625813506062-0aeb1d7a094b?w=600&auto=format&fit=crop&q=80', isAvailable: true },
  { id: 9, restaurantId: 2, name: 'Truffle Parmesan Loaded Fries', description: 'Hand-cut golden fries tossed with white truffle oil, grated parmesan, and chives.', price: 160, category: 'Sides', prepTimeMinutes: 10, calories: 420, imageUrl: 'https://images.unsplash.com/photo-1576107232684-1279f3908594?w=600&auto=format&fit=crop&q=80', isAvailable: true },
  { id: 11, restaurantId: 2, name: 'Belgian Chocolate Milkshake', description: 'Thick blended artisanal dark chocolate ice cream topped with chocolate drizzle.', price: 180, category: 'Beverages', prepTimeMinutes: 8, calories: 380, imageUrl: 'https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=600&auto=format&fit=crop&q=80', isAvailable: true },

  { id: 13, restaurantId: 3, name: 'Mediterranean Grilled Chicken Salad', description: 'Herb marinated chicken strips over mixed greens, olives, feta cheese, and lemon vinaigrette.', price: 310, category: 'Salads', prepTimeMinutes: 12, calories: 390, imageUrl: 'https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&auto=format&fit=crop&q=80', isAvailable: true },
  { id: 14, restaurantId: 3, name: 'Quinoa Avocado Power Bowl', description: 'Organic red quinoa, fresh Hass avocado, edamame, roasted chickpeas, and tahini drizzle.', price: 340, category: 'Grain Bowls', prepTimeMinutes: 12, calories: 460, imageUrl: 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=600&auto=format&fit=crop&q=80', isAvailable: true },
  { id: 16, restaurantId: 3, name: 'Cold Pressed Green Detox Juice', description: 'Pure blend of baby spinach, celery, green apple, cucumber, and ginger.', price: 140, category: 'Beverages', prepTimeMinutes: 5, calories: 95, imageUrl: 'https://images.unsplash.com/photo-1613478223719-2ab802602423?w=600&auto=format&fit=crop&q=80', isAvailable: true }
];

const DEFAULT_LEFTOVERS = [
  {
    id: 1,
    restaurantId: 1,
    restaurantName: 'SpiceCraft Kitchen',
    menuItemId: 1,
    itemName: 'Kacchi Biryani Special',
    category: 'Main Course',
    originalPrice: 380,
    discountPercent: 35,
    discountedPrice: 247,
    quantityAvailable: 4,
    expiresInHours: 3.5,
    imageUrl: 'https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=600&auto=format&fit=crop&q=80'
  },
  {
    id: 2,
    restaurantId: 2,
    restaurantName: 'Burger Barn & Grill',
    menuItemId: 8,
    itemName: 'Crispy Hot Buttermilk Chicken Burger',
    category: 'Burgers',
    originalPrice: 270,
    discountPercent: 30,
    discountedPrice: 189,
    quantityAvailable: 3,
    expiresInHours: 2.2,
    imageUrl: 'https://images.unsplash.com/photo-1625813506062-0aeb1d7a094b?w=600&auto=format&fit=crop&q=80'
  },
  {
    id: 3,
    restaurantId: 3,
    restaurantName: 'Green Bowls & Juices',
    menuItemId: 14,
    itemName: 'Quinoa Avocado Power Bowl',
    category: 'Grain Bowls',
    originalPrice: 340,
    discountPercent: 40,
    discountedPrice: 204,
    quantityAvailable: 5,
    expiresInHours: 4.8,
    imageUrl: 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=600&auto=format&fit=crop&q=80'
  }
];

const DEFAULT_ORDERS = [
  {
    id: 101,
    customerId: 5,
    customerName: 'Rahim Ahmed',
    customerPhone: '+8801711000005',
    deliveryAddress: 'Apartment 4B, Road 12, Dhanmondi, Dhaka',
    restaurantId: 1,
    restaurantName: 'SpiceCraft Kitchen',
    isCombined: false,
    combinedGroupCode: null,
    status: 'Delivered',
    totalAmount: 420,
    deliveryFee: 40,
    discountAmount: 0,
    paymentMethod: 'bKash',
    paymentStatus: 'Completed',
    riderId: 1,
    riderName: 'Tanvir Hasan (Rider 1)',
    createdAt: new Date(Date.now() - 1000 * 60 * 120).toISOString(),
    items: [
      { menuItemId: 1, name: 'Kacchi Biryani Special', quantity: 1, price: 380, subtotal: 380 }
    ]
  },
  {
    id: 102,
    customerId: 6,
    customerName: 'Fatima Jahan',
    customerPhone: '+8801711000006',
    deliveryAddress: 'House 88, Road 8, Dhanmondi, Dhaka',
    restaurantId: 1,
    restaurantName: 'SpiceCraft Kitchen',
    isCombined: false,
    combinedGroupCode: null,
    status: 'Preparing',
    totalAmount: 470,
    deliveryFee: 40,
    discountAmount: 0,
    paymentMethod: 'CashOnDelivery',
    paymentStatus: 'Pending',
    riderId: null,
    riderName: null,
    preferredDeliveryTime: new Date(Date.now() + 1000 * 60 * 45).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    createdAt: new Date(Date.now() - 1000 * 60 * 15).toISOString(),
    items: [
      { menuItemId: 2, name: 'Butter Chicken Masala', quantity: 1, price: 320, subtotal: 320 },
      { menuItemId: 4, name: 'Borhani Pitcher (500ml)', quantity: 1, price: 90, subtotal: 90 },
      { menuItemId: 3, name: 'Garlic Butter Naan', quantity: 1, price: 60, subtotal: 60 }
    ]
  },
  {
    id: 103,
    customerId: 6,
    customerName: 'Fatima Jahan',
    customerPhone: '+8801711000006',
    deliveryAddress: 'House 88, Road 8, Dhanmondi, Dhaka',
    restaurantId: 1,
    restaurantName: 'SpiceCraft Kitchen',
    isCombined: true,
    combinedGroupCode: 'COMB-20260906-01',
    status: 'Accepted',
    totalAmount: 410,
    deliveryFee: 30,
    discountAmount: 10,
    paymentMethod: 'bKash',
    paymentStatus: 'Completed',
    riderId: 1,
    riderName: 'Tanvir Hasan (Rider 1)',
    createdAt: new Date(Date.now() - 1000 * 60 * 8).toISOString(),
    items: [
      { menuItemId: 1, name: 'Kacchi Biryani Special', quantity: 1, price: 380, subtotal: 380 }
    ]
  },
  {
    id: 104,
    customerId: 6,
    customerName: 'Fatima Jahan',
    customerPhone: '+8801711000006',
    deliveryAddress: 'House 88, Road 8, Dhanmondi, Dhaka',
    restaurantId: 2,
    restaurantName: 'Burger Barn & Grill',
    isCombined: true,
    combinedGroupCode: 'COMB-20260906-01',
    status: 'Accepted',
    totalAmount: 210,
    deliveryFee: 30,
    discountAmount: 10,
    paymentMethod: 'bKash',
    paymentStatus: 'Completed',
    riderId: 1,
    riderName: 'Tanvir Hasan (Rider 1)',
    createdAt: new Date(Date.now() - 1000 * 60 * 8).toISOString(),
    items: [
      { menuItemId: 11, name: 'Belgian Chocolate Milkshake', quantity: 1, price: 180, subtotal: 180 }
    ]
  }
];

class StateManager {
  constructor() {
    this.loadState();
  }

  loadState() {
    const saved = localStorage.getItem('bitenest_state');
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        this.users = parsed.users || DEFAULT_USERS;
        this.restaurants = parsed.restaurants || DEFAULT_RESTAURANTS;
        this.menuItems = parsed.menuItems || DEFAULT_MENU_ITEMS;
        this.orders = parsed.orders || DEFAULT_ORDERS;
        this.leftovers = parsed.leftovers || DEFAULT_LEFTOVERS;
        this.cart = parsed.cart || [];
        this.currentUserId = parsed.currentUserId || 5; // Default Rahim Ahmed
        return;
      } catch (e) {
        console.warn('Failed to parse localStorage state, resetting to defaults.', e);
      }
    }

    this.users = [...DEFAULT_USERS];
    this.restaurants = [...DEFAULT_RESTAURANTS];
    this.menuItems = [...DEFAULT_MENU_ITEMS];
    this.orders = [...DEFAULT_ORDERS];
    this.leftovers = [...DEFAULT_LEFTOVERS];
    this.cart = [];
    this.currentUserId = 5;
    this.saveState();
  }

  saveState() {
    localStorage.setItem('bitenest_state', JSON.stringify({
      users: this.users,
      restaurants: this.restaurants,
      menuItems: this.menuItems,
      orders: this.orders,
      leftovers: this.leftovers,
      cart: this.cart,
      currentUserId: this.currentUserId
    }));
  }

  getCurrentUser() {
    return this.users.find(u => u.id === this.currentUserId) || this.users[4];
  }

  setCurrentUser(userId) {
    this.currentUserId = Number(userId);
    this.saveState();
  }

  // Calculate distance between two coordinates in km
  calculateDistance(lat1, lon1, lat2, lon2) {
    const R = 6371;
    const dLat = (lat2 - lat1) * (Math.PI / 180);
    const dLon = (lon2 - lon1) * (Math.PI / 180);
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
              Math.cos(lat1 * (Math.PI / 180)) * Math.cos(lat2 * (Math.PI / 180)) *
              Math.sin(dLon / 2) * Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return Math.round(R * c * 100) / 100;
  }

  // Smart Queue Status: Free / Normal / Busy
  getRestaurantQueueStatus(restaurantId) {
    const restaurant = this.restaurants.find(r => r.id === restaurantId);
    if (!restaurant) return { status: 'Free', activeCount: 0, waitMinutes: 15 };

    const activeOrders = this.orders.filter(o => 
      o.restaurantId === restaurantId && 
      ['Placed', 'Accepted', 'Preparing'].includes(o.status)
    ).length;

    const capacity = restaurant.kitchenCapacity || 15;
    let status = 'Free';
    if (activeOrders >= capacity) {
      status = 'Busy';
    } else if (activeOrders >= capacity / 3) {
      status = 'Normal';
    }

    const waitMinutes = restaurant.avgPrepTimeMinutes + (activeOrders * 3);
    return { status, activeCount: activeOrders, waitMinutes };
  }

  // Cart operations
  addToCart(menuItemId, quantity = 1) {
    const item = this.menuItems.find(m => m.id === menuItemId);
    if (!item) return;

    // Check if adding from a third restaurant
    const distinctRestaurants = new Set(this.cart.map(c => c.restaurantId));
    if (!distinctRestaurants.has(item.restaurantId) && distinctRestaurants.size >= 2) {
      return { success: false, message: 'Combined Delivery supports maximum 2 nearby restaurants in a single order.' };
    }

    // Check distance between the two restaurants if adding 2nd restaurant
    if (!distinctRestaurants.has(item.restaurantId) && distinctRestaurants.size === 1) {
      const existingRestId = Array.from(distinctRestaurants)[0];
      const r1 = this.restaurants.find(r => r.id === existingRestId);
      const r2 = this.restaurants.find(r => r.id === item.restaurantId);
      if (r1 && r2) {
        const dist = this.calculateDistance(r1.lat, r1.lng, r2.lat, r2.lng);
        if (dist > 2.0) {
          return { success: false, message: `These restaurants are ${dist} km apart (exceeding 2.0 km combined delivery threshold). Please order separately.` };
        }
      }
    }

    const existingIndex = this.cart.findIndex(c => c.menuItemId === menuItemId);
    if (existingIndex > -1) {
      this.cart[existingIndex].quantity += quantity;
    } else {
      const rest = this.restaurants.find(r => r.id === item.restaurantId);
      this.cart.push({
        menuItemId: item.id,
        restaurantId: item.restaurantId,
        restaurantName: rest?.name || 'Restaurant',
        name: item.name,
        price: item.price,
        quantity: quantity,
        calories: item.calories
      });
    }

    this.saveState();
    return { success: true };
  }

  removeFromCart(menuItemId) {
    this.cart = this.cart.filter(c => c.menuItemId !== menuItemId);
    this.saveState();
  }

  updateCartQuantity(menuItemId, quantity) {
    if (quantity <= 0) {
      this.removeFromCart(menuItemId);
    } else {
      const item = this.cart.find(c => c.menuItemId === menuItemId);
      if (item) {
        item.quantity = quantity;
        this.saveState();
      }
    }
  }

  clearCart() {
    this.cart = [];
    this.saveState();
  }

  getCartAnalysis() {
    const restaurantIds = [...new Set(this.cart.map(c => c.restaurantId))];
    const isCombined = restaurantIds.length === 2;
    const subtotal = this.cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    
    // Delivery fee logic
    // Single: 40 BDT
    // Combined: 60 BDT total (saving 20 BDT vs 2 x 40 = 80 BDT)
    let deliveryFee = 0;
    let savings = 0;
    if (this.cart.length > 0) {
      if (isCombined) {
        deliveryFee = 60;
        savings = 20;
      } else {
        deliveryFee = 40;
        savings = 0;
      }
    }

    return {
      restaurantCount: restaurantIds.length,
      isCombined,
      subtotal,
      deliveryFee,
      savings,
      total: subtotal + deliveryFee
    };
  }
}

window.state = new StateManager();
