import {
  AlertCircle,
  CreditCard,
  Lock,
  Package,
  ShoppingCart,
  User,
} from "lucide-react"
import type { ElementType } from "react"
import type { ChatInfo, ManualAction } from "./types"

export interface ChatInfoPanelProps {
  chatInfo: ChatInfo
}

const actionIcons: Record<ManualAction["action"], ElementType> = {
  upsell: AlertCircle,
  "payment-link": CreditCard,
  shipping: Package,
}

export function ChatInfoPanel({ chatInfo }: ChatInfoPanelProps) {
  const cartTotal = chatInfo.cartItems.reduce((total, item) => {
    return total + item.price * item.quantity
  }, 0)

  return (
    <section className="flex h-full min-h-0 flex-col gap-4 overflow-y-auto bg-gray-50 p-4 dark:bg-gray-925">
      <article className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm dark:border-gray-800 dark:bg-gray-950">
        <span className="mb-4 flex items-center text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">
          <User className="mr-2 size-3.5" /> Customer Info
        </span>
        <div className="space-y-3">
          <div>
            <p className="font-medium text-gray-900 dark:text-gray-50">
              {chatInfo.customer.name}
            </p>
            <p className="text-sm text-gray-500 dark:text-gray-400">
              {chatInfo.customer.contact}
            </p>
          </div>
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-500 dark:text-gray-400">LTV</span>
            <span className="font-medium text-gray-900 dark:text-gray-50">
              ${chatInfo.customer.lifetimeValue.toFixed(2)}
            </span>
          </div>
          <div className="flex flex-wrap gap-2">
            {chatInfo.customer.tags.map((tag) => (
              <span
                key={tag}
                className="rounded-md bg-gray-100 px-1.5 py-0.5 text-[10px] font-medium text-gray-700 dark:bg-gray-800 dark:text-gray-300"
              >
                {tag}
              </span>
            ))}
          </div>
        </div>
      </article>

      <article className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm dark:border-gray-800 dark:bg-gray-950">
        <span className="mb-4 flex items-center text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">
          <ShoppingCart className="mr-2 size-3.5" /> Active Cart
        </span>
        {chatInfo.cartItems.length === 0 ? (
          <p className="text-sm italic text-gray-500 dark:text-gray-400">
            Cart is empty.
          </p>
        ) : (
          <>
            <ul className="divide-y divide-gray-100 dark:divide-gray-900">
              {chatInfo.cartItems.map((item) => (
                <li
                  key={item.id}
                  className="flex items-center justify-between py-3"
                >
                  <div>
                    <p className="font-medium text-gray-900 dark:text-gray-50">
                      {item.name}
                    </p>
                    <p className="text-xs text-gray-500 dark:text-gray-400">
                      {item.variant} x {item.quantity}
                    </p>
                  </div>
                  <span className="font-medium text-gray-900 dark:text-gray-50">
                    ${item.price.toFixed(2)}
                  </span>
                </li>
              ))}
            </ul>
            <div className="my-4 border-t border-gray-200 dark:border-gray-800" />
            <div className="flex items-center justify-between font-semibold text-gray-900 dark:text-gray-50">
              <span>Total</span>
              <span>${cartTotal.toFixed(2)}</span>
            </div>
          </>
        )}
      </article>

      <article className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm dark:border-gray-800 dark:bg-gray-950">
        <span className="mb-4 flex items-center text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">
          <Lock className="mr-2 size-3.5" /> Manual Actions
        </span>
        <div className="space-y-2">
          {chatInfo.manualActions.map((action) => {
            const Icon = actionIcons[action.action]
            return (
              <button
                key={action.id}
                type="button"
                className="inline-flex w-full items-center justify-between rounded-md border border-gray-200 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 dark:border-gray-800 dark:bg-gray-950 dark:text-gray-300 dark:hover:bg-gray-900"
              >
                <span className="inline-flex items-center">
                  <Icon className="mr-2 size-4 text-gray-400 dark:text-gray-500" />
                  {action.label}
                </span>
              </button>
            )
          })}
        </div>
      </article>

      <article className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm dark:border-gray-800 dark:bg-gray-950">
        <span className="mb-4 block text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">
          Past Orders
        </span>
        <ul className="divide-y divide-gray-100 dark:divide-gray-900">
          {chatInfo.pastOrders.map((order) => (
            <li key={order.id} className="space-y-1 py-3 text-xs">
              <div className="flex items-center justify-between gap-2">
                <span className="font-semibold text-gray-900 dark:text-gray-50">
                  {order.id}
                </span>
                <span className="text-gray-500 dark:text-gray-400">
                  {order.dateLabel}
                </span>
              </div>
              <div className="flex items-center justify-between gap-2">
                <span className="font-medium text-gray-600 dark:text-gray-300">
                  ${order.total.toFixed(2)}
                </span>
                <span className="uppercase tracking-wide text-gray-500 dark:text-gray-400">
                  {order.status}
                </span>
              </div>
            </li>
          ))}
        </ul>
      </article>
    </section>
  )
}
