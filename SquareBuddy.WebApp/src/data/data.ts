interface AssignedPerson {
  name: string
  initials: string
}

interface Project {
  company: string
  size: string
  probability: string
  duration: string
  status: "Drafted" | "Sent" | "Closed"
  assigned: AssignedPerson[]
}

interface Region {
  region: string
  project: Project[]
}

export const quotes: Region[] = [
  {
    region: "Europe",
    project: [
      {
        company: "Walton Holding",
        size: "50K USD",
        probability: "40%",
        duration: "18 months",
        status: "Drafted",
        assigned: [
          {
            name: "Emily Smith",
            initials: "E",
          },
          {
            name: "Max Warmer",
            initials: "M",
          },
          {
            name: "Victoria Steep",
            initials: "V",
          },
        ],
      },
      {
        company: "Zurich Coats LLC",
        size: "100-150K USD",
        probability: "80%",
        duration: "24 months",
        status: "Sent",
        assigned: [
          {
            name: "Emma Stone",
            initials: "E",
          },
          {
            name: "Chris Bold",
            initials: "C",
          },
        ],
      },
      {
        company: "Riverflow Media Group",
        size: "280-300K USD",
        probability: "80%",
        duration: "24 months",
        status: "Sent",
        assigned: [
          {
            name: "Emma Stephcorn",
            initials: "E",
          },
          {
            name: "Chris Bold",
            initials: "C",
          },
        ],
      },
      {
        company: "Nordic Solutions AG",
        size: "175K USD",
        probability: "60%",
        duration: "12 months",
        status: "Drafted",
        assigned: [
          {
            name: "Victoria Stone",
            initials: "V",
          },
          {
            name: "Max W.",
            initials: "M",
          },
        ],
      },
      {
        company: "Swiss Tech Innovations",
        size: "450K USD",
        probability: "90%",
        duration: "36 months",
        status: "Sent",
        assigned: [
          {
            name: "Emily Satally",
            initials: "E",
          },
          {
            name: "Chris Bold",
            initials: "C",
          },
        ],
      },
      {
        company: "Berlin Digital Hub",
        size: "200K USD",
        probability: "70%",
        duration: "15 months",
        status: "Drafted",
        assigned: [
          {
            name: "Emma Stone",
            initials: "E",
          },
        ],
      },
    ],
  },
  {
    region: "Asia",
    project: [
      {
        company: "Real Estate Group",
        size: "1.2M USD",
        probability: "100%",
        duration: "6 months",
        status: "Closed",
        assigned: [
          {
            name: "Lena Mayer",
            initials: "L",
          },
          {
            name: "Sara Brick",
            initials: "S",
          },
        ],
      },
      {
        company: "Grison Appartments",
        size: "100K USD",
        probability: "20%",
        duration: "12 months",
        status: "Drafted",
        assigned: [
          {
            name: "Jordan Afolter",
            initials: "J",
          },
          {
            name: "Corinna Bridge",
            initials: "C",
          },
        ],
      },
      {
        company: "Tokyo Tech Solutions",
        size: "750K USD",
        probability: "85%",
        duration: "24 months",
        status: "Sent",
        assigned: [
          {
            name: "Lena Mayer",
            initials: "L",
          },
          {
            name: "Jordan Corner",
            initials: "J",
          },
        ],
      },
      {
        company: "Singapore Systems Ltd",
        size: "300K USD",
        probability: "75%",
        duration: "18 months",
        status: "Drafted",
        assigned: [
          {
            name: "Sara Bridge",
            initials: "S",
          },
        ],
      },
      {
        company: "Seoul Digital Corp",
        size: "880K USD",
        probability: "95%",
        duration: "30 months",
        status: "Sent",
        assigned: [
          {
            name: "Corinna Berner",
            initials: "C",
          },
          {
            name: "Lena Mayer",
            initials: "L",
          },
        ],
      },
      {
        company: "Mumbai Innovations",
        size: "450K USD",
        probability: "40%",
        duration: "12 months",
        status: "Drafted",
        assigned: [
          {
            name: "Jordan Afolter",
            initials: "J",
          },
        ],
      },
    ],
  },
  {
    region: "North America",
    project: [
      {
        company: "Liquid Holdings Group",
        size: "5.1M USD",
        probability: "100%",
        duration: "Member",
        status: "Closed",
        assigned: [
          {
            name: "Charlie Anuk",
            initials: "C",
          },
        ],
      },
      {
        company: "Craft Labs, Inc.",
        size: "80-90K USD",
        probability: "80%",
        duration: "18 months",
        status: "Sent",
        assigned: [
          {
            name: "Charlie Anuk",
            initials: "C",
          },
          {
            name: "Patrick Daller",
            initials: "P",
          },
        ],
      },
      {
        company: "Toronto Tech Hub",
        size: "250K USD",
        probability: "65%",
        duration: "12 months",
        status: "Drafted",
        assigned: [
          {
            name: "Patrick Daller",
            initials: "P",
          },
          {
            name: "Charlie Anuk",
            initials: "C",
          },
        ],
      },
      {
        company: "Silicon Valley Startups",
        size: "1.5M USD",
        probability: "90%",
        duration: "24 months",
        status: "Sent",
        assigned: [
          {
            name: "Charlie Anuk",
            initials: "C",
          },
        ],
      },
      {
        company: "NYC Digital Solutions",
        size: "750K USD",
        probability: "70%",
        duration: "15 months",
        status: "Drafted",
        assigned: [
          {
            name: "Patrick Daller",
            initials: "P",
          },
        ],
      },
    ],
  },
]

interface DataChart {
  date: string
  "Current year": number
  "Same period last year": number
}

interface DataChart2 {
  date: string
  Quotes: number
  "Total deal size": number
}

interface DataChart3 {
  date: string
  Addressed: number
  Unrealized: number
}

interface DataChart4 {
  date: string
  Density: number
}

export const dataChart: DataChart[] = [
  {
    date: "Jan 24",
    "Current year": 23,
    "Same period last year": 67,
  },
  {
    date: "Feb 24",
    "Current year": 31,
    "Same period last year": 23,
  },
  {
    date: "Mar 24",
    "Current year": 46,
    "Same period last year": 78,
  },
  {
    date: "Apr 24",
    "Current year": 46,
    "Same period last year": 23,
  },
  {
    date: "May 24",
    "Current year": 39,
    "Same period last year": 32,
  },
  {
    date: "Jun 24",
    "Current year": 65,
    "Same period last year": 32,
  },
]

export const dataChart2: DataChart2[] = [
  {
    date: "Jan 24",
    Quotes: 120,
    "Total deal size": 55000,
  },
  {
    date: "Feb 24",
    Quotes: 183,
    "Total deal size": 75400,
  },
  {
    date: "Mar 24",
    Quotes: 165,
    "Total deal size": 50450,
  },
  {
    date: "Apr 24",
    Quotes: 99,
    "Total deal size": 41540,
  },
  {
    date: "May 24",
    Quotes: 194,
    "Total deal size": 63850,
  },
  {
    date: "Jun 24",
    Quotes: 241,
    "Total deal size": 73850,
  },
]

export const dataChart3: DataChart3[] = [
  {
    date: "Jan 24",
    Addressed: 8,
    Unrealized: 12,
  },
  {
    date: "Feb 24",
    Addressed: 9,
    Unrealized: 12,
  },
  {
    date: "Mar 24",
    Addressed: 6,
    Unrealized: 12,
  },
  {
    date: "Apr 24",
    Addressed: 5,
    Unrealized: 12,
  },
  {
    date: "May 24",
    Addressed: 12,
    Unrealized: 12,
  },
  {
    date: "Jun 24",
    Addressed: 9,
    Unrealized: 12,
  },
]

export const dataChart4: DataChart4[] = [
  {
    date: "Jan 24",
    Density: 0.891,
  },
  {
    date: "Feb 24",
    Density: 0.084,
  },
  {
    date: "Mar 24",
    Density: 0.155,
  },
  {
    date: "Apr 24",
    Density: 0.75,
  },
  {
    date: "May 24",
    Density: 0.221,
  },
  {
    date: "Jun 24",
    Density: 0.561,
  },
]

interface Progress {
  current: number
  total: number
}

interface AuditDate {
  date: string
  auditor: string
}

interface Document {
  name: string
  status: "OK" | "Needs update" | "In audit"
}

interface Section {
  id: string
  title: string
  certified: string
  progress: Progress
  status: "complete" | "warning"
  auditDates: AuditDate[]
  documents: Document[]
}

export const sections: Section[] = [
  {
    id: "item-1",
    title: "CompTIA Security+",
    certified: "ISO",
    progress: { current: 46, total: 46 },
    status: "complete",
    auditDates: [
      { date: "Dec 10, 2023", auditor: "Max Duster" },
      { date: "Dec 12, 2023", auditor: "Emma Stone" },
    ],
    documents: [
      { name: "policy_overview.xlsx", status: "OK" },
      { name: "employee_guidelines.xlsx", status: "Needs update" },
      { name: "compliance_checklist.xlsx", status: "In audit" },
    ],
  },
  {
    id: "item-2",
    title: "SAFe Certifications",
    certified: "IEC 2701",
    progress: { current: 32, total: 41 },
    status: "warning",
    auditDates: [
      { date: "Jan 15, 2024", auditor: "Sarah Johnson" },
      { date: "Jan 20, 2024", auditor: "Mike Peters" },
    ],
    documents: [
      { name: "certification_records.xlsx", status: "OK" },
      { name: "training_logs.xlsx", status: "In audit" },
      { name: "assessment_results.xlsx", status: "Needs update" },
    ],
  },
  {
    id: "item-3",
    title: "PMP Certifications",
    certified: "ISO",
    progress: { current: 21, total: 21 },
    status: "complete",
    auditDates: [
      { date: "Feb 5, 2024", auditor: "Lisa Chen" },
      { date: "Feb 8, 2024", auditor: "Tom Wilson" },
    ],
    documents: [
      { name: "project_documents.xlsx", status: "OK" },
      { name: "methodology_guide.xlsx", status: "OK" },
      { name: "best_practices.xlsx", status: "In audit" },
    ],
  },
  {
    id: "item-4",
    title: "Cloud Certifications",
    certified: "SOC 2",
    progress: { current: 21, total: 21 },
    status: "complete",
    auditDates: [
      { date: "Mar 1, 2024", auditor: "Alex Kumar" },
      { date: "Mar 5, 2024", auditor: "Rachel Green" },
    ],
    documents: [
      { name: "aws_certifications.xlsx", status: "OK" },
      { name: "azure_competencies.xlsx", status: "OK" },
      { name: "gcp_credentials.xlsx", status: "In audit" },
      { name: "cloud_security.xlsx", status: "OK" },
    ],
  },
]

export type ConversationChannel = "WhatsApp" | "Telegram" | "SMS"

export type MessageSender = "user" | "agent"

export interface ConversationItem {
  id: string
  name: string
  channel: ConversationChannel
  lastMessage: string
  timeLabel: string
  hasUnread: boolean
}

export interface MessageItem {
  id: string
  conversationId: string
  sender: MessageSender
  text: string
  timeLabel: string
}

export interface CartItem {
  id: string
  name: string
  variant: string
  price: number
  quantity: number
}

export interface PastOrder {
  id: string
  dateLabel: string
  total: number
  status: string
}

export interface ManualAction {
  id: string
  label: string
  action: "upsell" | "payment-link" | "shipping"
}

export interface CustomerInfo {
  name: string
  contact: string
  lifetimeValue: number
  tags: string[]
}

export interface ChatInfo {
  conversationId: string
  customer: CustomerInfo
  cartItems: CartItem[]
  manualActions: ManualAction[]
  pastOrders: PastOrder[]
}

export interface ConversationOverviewData {
  conversations: ConversationItem[]
  messages: MessageItem[]
  chatInfo: ChatInfo[]
}

export const conversationOverviewData: ConversationOverviewData = {
  conversations: [
    {
      id: "1",
      name: "+1 555-0101",
      channel: "WhatsApp",
      lastMessage: "Is the Pro version in stock?",
      timeLabel: "10:42 AM",
      hasUnread: true,
    },
    {
      id: "2",
      name: "@alex_dev",
      channel: "Telegram",
      lastMessage: "Payment completed.",
      timeLabel: "09:15 AM",
      hasUnread: false,
    },
    {
      id: "3",
      name: "+44 7700 900077",
      channel: "SMS",
      lastMessage: "When will it ship?",
      timeLabel: "Yesterday",
      hasUnread: false,
    },
  ],
  messages: [
    {
      id: "m1",
      conversationId: "1",
      sender: "user",
      text: "Hi, I want to order the mechanical keyboard.",
      timeLabel: "10:40 AM",
    },
    {
      id: "m2",
      conversationId: "1",
      sender: "agent",
      text: "Hello! I can help with that. Do you want tactile or linear switches?",
      timeLabel: "10:40 AM",
    },
    {
      id: "m3",
      conversationId: "1",
      sender: "user",
      text: "Tactile, please. Is the Pro version in stock?",
      timeLabel: "10:42 AM",
    },
    {
      id: "m4",
      conversationId: "2",
      sender: "agent",
      text: "Your payment link was confirmed. Thanks for your order.",
      timeLabel: "09:10 AM",
    },
    {
      id: "m5",
      conversationId: "2",
      sender: "user",
      text: "Payment completed.",
      timeLabel: "09:15 AM",
    },
    {
      id: "m6",
      conversationId: "3",
      sender: "user",
      text: "When will it ship?",
      timeLabel: "Yesterday",
    },
    {
      id: "m7",
      conversationId: "3",
      sender: "agent",
      text: "We can ship tomorrow morning once payment is verified.",
      timeLabel: "Yesterday",
    },
  ],
  chatInfo: [
    {
      conversationId: "1",
      customer: {
        name: "Unknown User",
        contact: "+1 555-0101",
        lifetimeValue: 165.5,
        tags: ["Returning", "Tech"],
      },
      cartItems: [
        {
          id: "c1",
          name: "Mech Keyboard Pro",
          variant: "Tactile / Black",
          price: 149.99,
          quantity: 1,
        },
      ],
      manualActions: [
        {
          id: "a1",
          label: "Trigger Upsell",
          action: "upsell",
        },
        {
          id: "a2",
          label: "Send Payment Link",
          action: "payment-link",
        },
        {
          id: "a3",
          label: "Schedule Shipping",
          action: "shipping",
        },
      ],
      pastOrders: [
        {
          id: "ORD-882",
          dateLabel: "Oct 12, 2025",
          total: 45,
          status: "Delivered",
        },
        {
          id: "ORD-751",
          dateLabel: "Aug 04, 2025",
          total: 120.5,
          status: "Delivered",
        },
      ],
    },
    {
      conversationId: "2",
      customer: {
        name: "Alex Dev",
        contact: "@alex_dev",
        lifetimeValue: 320.75,
        tags: ["VIP", "Telegram"],
      },
      cartItems: [
        {
          id: "c2",
          name: "Wireless Mouse",
          variant: "Graphite",
          price: 79,
          quantity: 1,
        },
      ],
      manualActions: [
        {
          id: "a4",
          label: "Trigger Upsell",
          action: "upsell",
        },
        {
          id: "a5",
          label: "Send Payment Link",
          action: "payment-link",
        },
        {
          id: "a6",
          label: "Schedule Shipping",
          action: "shipping",
        },
      ],
      pastOrders: [
        {
          id: "ORD-912",
          dateLabel: "Jan 03, 2026",
          total: 99.99,
          status: "Delivered",
        },
      ],
    },
    {
      conversationId: "3",
      customer: {
        name: "UK Customer",
        contact: "+44 7700 900077",
        lifetimeValue: 58.2,
        tags: ["SMS", "First-Time"],
      },
      cartItems: [],
      manualActions: [
        {
          id: "a7",
          label: "Trigger Upsell",
          action: "upsell",
        },
        {
          id: "a8",
          label: "Send Payment Link",
          action: "payment-link",
        },
        {
          id: "a9",
          label: "Schedule Shipping",
          action: "shipping",
        },
      ],
      pastOrders: [
        {
          id: "ORD-644",
          dateLabel: "Jul 30, 2025",
          total: 58.2,
          status: "Delivered",
        },
      ],
    },
  ],
}
